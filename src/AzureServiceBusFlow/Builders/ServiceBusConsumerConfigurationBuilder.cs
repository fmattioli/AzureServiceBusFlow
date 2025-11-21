using Azure.Messaging.ServiceBus;
using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Hosts;
using AzureServiceBusFlow.Middlewares;
using AzureServiceBusFlow.Models;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureServiceBusFlow.Builders
{
    public class ServiceBusConsumerConfigurationBuilder(AzureServiceBusConfiguration _azureServiceBusConfiguration, IServiceCollection services)
    {
        private readonly string _connectionString = _azureServiceBusConfiguration.ConnectionString;
        private readonly IServiceCollection _services = services;
        private readonly Dictionary<Type, List<Type>> _handlers = [];

        private string? _queueName;
        private string? _topicName;
        private string? _subscriptionName;
        private readonly List<Type> _middlewares = [];
        private readonly string _consumerMiddlewareKey = Guid.NewGuid().ToString();

        public ServiceBusConsumerConfigurationBuilder UseMiddleware<TMiddleware>()
            where TMiddleware : IConsumerMiddleware
        {
            if (!_middlewares.Contains(typeof(TMiddleware)))
            {
                _middlewares.Add(typeof(TMiddleware));
            }

            return this;
        }

        public ServiceBusConsumerConfigurationBuilder FromQueue(string queueName)
        {
            _queueName = queueName;
            return this;
        }

        public ServiceBusConsumerConfigurationBuilder FromTopic(string topicName, string subscriptionName)
        {
            _topicName = topicName;
            _subscriptionName = subscriptionName;
            return this;
        }

        public ServiceBusConsumerConfigurationBuilder EnsureSubscriptionExists(string topicName, string subscriptionName)
        {
            var managementClient = new ManagementClient(_connectionString);
            if (!managementClient.SubscriptionExistsAsync(topicName, subscriptionName).GetAwaiter().GetResult())
            {
                managementClient.CreateSubscriptionAsync(topicName, subscriptionName).GetAwaiter().GetResult();
            }

            managementClient.CloseAsync().GetAwaiter().GetResult();
            return this;
        }

        public ServiceBusConsumerConfigurationBuilder AddHandler<TMessage, THandler>()
            where TMessage : class, IServiceBusMessage
            where THandler : class, IMessageHandler<TMessage>
        {
            var messageType = typeof(TMessage);
            var handlerType = typeof(THandler);

            if (!_handlers.TryGetValue(messageType, out var handlerList))
            {
                handlerList = [];
                _handlers[messageType] = handlerList;
            }

            if (!handlerList.Contains(handlerType))
            {
                handlerList.Add(handlerType);
            }

            _services.AddScoped<IMessageHandler<TMessage>, THandler>();
            _services.AddScoped<THandler>();

            return this;
        }

        public void Build()
        {
            if (string.IsNullOrWhiteSpace(_queueName) && string.IsNullOrWhiteSpace(_topicName))
            {
                throw new InvalidOperationException("Missing queue or topic configuration!");
            }

            foreach (var middlewareType in from middlewareType in _middlewares
                                           where !_services.Any(s =>
                                           s.ServiceType == typeof(IConsumerMiddleware) &&
                                           s.ImplementationType == middlewareType)
                                           select middlewareType)
            {
                _services.AddKeyedSingleton(typeof(IConsumerMiddleware), _consumerMiddlewareKey,  middlewareType);
            }

            _services.AddSingleton<IHostedService>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ServiceBusConsumerHostedService>>();
                var localConsumerMiddlewares = sp.GetKeyedServices<IConsumerMiddleware>(_consumerMiddlewareKey);
                var globalConsumerMiddlewares = sp.GetServices<IConsumerMiddleware>();

                var consumerMiddlewares = globalConsumerMiddlewares.Union(localConsumerMiddlewares);

                if (!string.IsNullOrWhiteSpace(_queueName))
                {
                    return new ServiceBusConsumerHostedService(
                        (rawMessage, rootProvider, cancellationToken) =>
                            MessageConsumingHandler(rawMessage, rootProvider, consumerMiddlewares, logger, cancellationToken),
                        sp,
                        logger,
                        _azureServiceBusConfiguration,
                        _queueName!);
                }

                return new ServiceBusConsumerHostedService(
                    (rawMessage, rootProvider, cancellationToken) =>
                        MessageConsumingHandler(rawMessage, rootProvider, consumerMiddlewares, logger, cancellationToken),
                    sp,
                    logger,
                    _azureServiceBusConfiguration,
                    _topicName!,
                    _subscriptionName!);
            });
        }

        private async Task MessageConsumingHandler(ServiceBusReceivedMessage rawMessage, IServiceProvider rootProvider, IEnumerable<IConsumerMiddleware> middlewares, ILogger<ServiceBusConsumerHostedService> logger, CancellationToken cancellationToken)
        {
            Func<Task> finalStep = async () =>
            {
                await ProcessHandlersAsync(rawMessage, rootProvider, logger, cancellationToken);
            };

            if (middlewares != null && middlewares.Any())
            {
                Func<Task> next = finalStep;

                foreach (var middleware in middlewares.Reverse())
                {
                    var current = middleware;
                    var prevNext = next;

                    next = () => current.InvokeAsync(rawMessage, prevNext, cancellationToken);
                }

                await next();
            }
            else
            {
                await finalStep();
            }
        }

        private async Task ProcessHandlersAsync(ServiceBusReceivedMessage rawMessage, IServiceProvider rootProvider, ILogger<ServiceBusConsumerHostedService> logger, CancellationToken cancellationToken)
        {
            if (!rawMessage.ApplicationProperties.TryGetValue("MessageType", out var messageTypeNameObj))
            {
                logger.LogWarning("MessageType property not found on message at {Time}", DateTime.UtcNow);
                return;
            }

            var messageTypeName = messageTypeNameObj as string;
            if (string.IsNullOrWhiteSpace(messageTypeName))
            {
                logger.LogWarning("MessageType property is null or empty at {Time}", DateTime.UtcNow);
                return;
            }

            var messageType = _handlers.Keys.FirstOrDefault(t =>
                string.Equals(t.FullName, messageTypeName, StringComparison.Ordinal) || string.Equals(t.Name, messageTypeName, StringComparison.Ordinal));

            if (messageType == null)
            {
                logger.LogWarning(
                    "Received message of type {MessageType}, but no handler is registered to process this message. Time: {Time}",
                    messageTypeName, DateTime.UtcNow
                );

                return;
            }

            var json = rawMessage.Body.ToString();
            var obj = JsonConvert.DeserializeObject(json, messageType);
            if (obj == null)
            {
                logger.LogWarning("Failed to deserialize message of type {MessageType} at {Time}", messageTypeName, DateTime.UtcNow);
                return;
            }

            var handlerTypes = _handlers[messageType];

            foreach (var handlerType in handlerTypes)
            {
                using var scope = rootProvider.CreateScope();

                var handler = scope.ServiceProvider.GetRequiredService(handlerType);

                var handlerInterface = handlerType.GetInterfaces()
                    .ToList()
                    .Find(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IMessageHandler<>) &&
                        i.GenericTypeArguments[0] == messageType);

                if (handlerInterface == null)
                {
                    logger.LogWarning(
                        "Handler {HandlerName} does not implement IMessageHandler<> as expected. Time: {Time}",
                        handlerType.Name, DateTime.UtcNow
                    );
                    continue;
                }

                var startTime = DateTime.UtcNow;
                var method = handlerInterface.GetMethod("HandleAsync");
                await (Task)method!.Invoke(handler, [obj!, rawMessage, cancellationToken])!;

                var elapsed = DateTime.UtcNow - startTime;

                logger.LogInformation(
                    "Message {MessageType} with CorrelationId {CorrelationId} consumed and handled by {HandlerName} at {StartTime} in {ElapsedMilliseconds} ms",
                    messageTypeName,
                    rawMessage.CorrelationId,
                    handlerType.Name,
                    startTime.ToString("o"),
                    elapsed.TotalMilliseconds
                );
            }
        }
    }
}
