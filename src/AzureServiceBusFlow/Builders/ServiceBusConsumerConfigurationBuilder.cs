using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Hosts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AzureServiceBusFlow.Builders
{
    public class ServiceBusConsumerConfigurationBuilder<TMessage>(string connectionString, IServiceCollection _services)
        where TMessage : class, IServiceBusMessage
    {
        private readonly List<Type> _handlerTypes = [];
        private readonly string _connectionString = connectionString;

        private string? _queueName;
        private string? _topicName;
        private string? _subscriptionName;

        public ServiceBusConsumerConfigurationBuilder<TMessage> FromQueue(string queueName)
        {
            _queueName = queueName;
            return this;
        }

        public ServiceBusConsumerConfigurationBuilder<TMessage> FromTopic(string topicName, string subscriptionName)
        {
            _topicName = topicName;
            _subscriptionName = subscriptionName;
            return this;
        }

        public ServiceBusConsumerConfigurationBuilder<TMessage> AddHandler<THandler>()
            where THandler : class, IMessageHandler<TMessage>
        {
            _handlerTypes.Add(typeof(THandler));
            _services.AddScoped<IMessageHandler<TMessage>, THandler>();
            _services.AddScoped<THandler>();
            return this;
        }


        public void Build()
        {
            if (string.IsNullOrWhiteSpace(_queueName) && (string.IsNullOrWhiteSpace(_topicName) || string.IsNullOrWhiteSpace(_subscriptionName)))
                throw new InvalidOperationException("Either queue or both topic/subscription must be configured.");

            _services.AddSingleton<IHostedService>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ServiceBusQueueConsumerHostedService<TMessage>>>();

                async Task Handler(TMessage message, IServiceProvider rootProvider)
                {
                    using var scope = rootProvider.CreateScope();

                    foreach (var handlerType in _handlerTypes)
                    {
                        var handler = (IMessageHandler<TMessage>)scope.ServiceProvider.GetRequiredService(handlerType);
                        await handler.HandleAsync(message, CancellationToken.None);
                    }
                }

                if (!string.IsNullOrWhiteSpace(_queueName))
                {
                    return new ServiceBusQueueConsumerHostedService<TMessage>(
                        _connectionString,
                        _queueName!,
                        Handler,
                        sp,
                        logger);
                }

                return new ServiceBusTopicConsumerHostedService<TMessage>(
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
