using Azure.Messaging.ServiceBus;
using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Hosts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureServiceBusFlow.Builders
{
    public class ServiceBusConsumerConfigurationBuilder(string connectionString, IServiceCollection services)
    {
        private readonly string _connectionString = connectionString;
        private readonly IServiceCollection _services = services;
        private readonly Dictionary<Type, List<Type>> _handlers = [];

        private string? _queueName;
        private string? _topicName;
        private string? _subscriptionName;

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

            _ = _services.AddSingleton<IHostedService>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ServiceBusQueueConsumerHostedService>>();

                async Task MessageConsumingHandler(ServiceBusReceivedMessage rawMessage, IServiceProvider rootProvider)
                {
                    try
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

                        var messageType = _handlers.Keys.FirstOrDefault(t => t.Name == messageTypeName);
                        if (messageType == null)
                        {
                            logger.LogWarning(
                                "Received message of type {MessageType}, but no handler is registered. Time: {Time}",
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

                            logger.LogInformation("Routing message to handler {HandlerName} at {Time}", handlerType.Name, DateTime.UtcNow);

                            var method = handlerInterface.GetMethod("HandleAsync");
                            await (Task)method!.Invoke(handler, [obj!, rawMessage])!;

                            logger.LogInformation("Message processed successfully by handler {HandlerName} at {Time}", handlerType.Name, DateTime.UtcNow);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing message in handler");
                        throw new InvalidOperationException("Error while trying to process message.", ex);
                    }
                }

                if (!string.IsNullOrWhiteSpace(_queueName))
                {
                    return new ServiceBusQueueConsumerHostedService(
                        _connectionString,
                        _queueName!,
                        MessageConsumingHandler,
                        sp,
                        logger);
                }

                return new ServiceBusTopicConsumerHostedService(
                    _connectionString,
                    _topicName!,
                    _subscriptionName!,
                    MessageConsumingHandler,
                    sp,
                    logger);
            });
        }
    }

}
