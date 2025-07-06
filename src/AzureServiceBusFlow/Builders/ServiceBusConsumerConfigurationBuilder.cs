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
        private readonly Dictionary<Type, Type> _handlers = [];

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
            _handlers[typeof(TMessage)] = typeof(THandler);
            _services.AddScoped<IMessageHandler<TMessage>, THandler>();
            _services.AddScoped<THandler>();
            return this;
        }

        public void Build()
        {
            if (string.IsNullOrWhiteSpace(_queueName) && string.IsNullOrWhiteSpace(_topicName))
                throw new InvalidOperationException("Missing queue or topic configuration!");

            _ = _services.AddSingleton<IHostedService>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ServiceBusQueueConsumerHostedService>>();

                async Task Handler(ServiceBusReceivedMessage rawMessage, IServiceProvider rootProvider)
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

                        // Tenta encontrar tipo de mensagem registrado
                        var messageType = _handlers.Keys.FirstOrDefault(t => t.Name == messageTypeName);
                        if (messageType == null)
                        {
                            logger.LogInformation(
                                "Received message of type {MessageType}, but no handler is registered to process it. Message will be ignored. Time: {Time}",
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

                        using var scope = rootProvider.CreateScope();
                        var handlerType = _handlers[messageType];
                        var handler = scope.ServiceProvider.GetRequiredService(handlerType);

                        var handlerWithRawInterface = handlerType.GetInterfaces()
                            .FirstOrDefault(i =>
                                i.IsGenericType &&
                                i.GetGenericTypeDefinition() == typeof(IMessageHandler<>) &&
                                i.GenericTypeArguments[0] == messageType);

                        if (handlerWithRawInterface == null)
                        {
                            logger.LogWarning(
                                "Handler for message type {MessageType} does not implement IMessageHandler<> as expected. Time: {Time}",
                                messageTypeName, DateTime.UtcNow
                            );
                            return;
                        }

                        logger.LogInformation("Message routed to handler {HandlerName} at {Time}", handlerType.Name, DateTime.UtcNow);
                        var method = handlerWithRawInterface.GetMethod("HandleAsync");
                        await (Task)method!.Invoke(handler, [obj!, rawMessage])!;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing message in handler");
                        throw new InvalidOperationException("Error while trying procces message.", ex);
                    }
                }

                if (!string.IsNullOrWhiteSpace(_queueName))
                {
                    return new ServiceBusQueueConsumerHostedService(
                        _connectionString,
                        _queueName!,
                        Handler,
                        sp,
                        logger);
                }

                return new ServiceBusTopicConsumerHostedService(
                    _connectionString,
                    _topicName!,
                    _subscriptionName!,
                    Handler,
                    sp,
                    logger);
            });
        }
    }
}
