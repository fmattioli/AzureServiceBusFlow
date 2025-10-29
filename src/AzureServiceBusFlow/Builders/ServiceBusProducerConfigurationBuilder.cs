using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Middlewar;
using AzureServiceBusFlow.Models;
using AzureServiceBusFlow.Producers;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureServiceBusFlow.Builders
{
    public class ServiceBusProducerConfigurationBuilder<TMessage>(AzureServiceBusConfiguration azureServiceBusConfiguration, IServiceCollection services)
    where TMessage : class, IServiceBusMessage
    {
        private readonly AzureServiceBusConfiguration _azureServiceBusConfiguration = azureServiceBusConfiguration;
        private readonly IServiceCollection _services = services;

        private string? _topicName;
        private string? _queueName;
        private readonly List<Type> _middlewares = [];

        public ServiceBusProducerConfigurationBuilder<TMessage> UseMiddleware<TMiddleware>()
            where TMiddleware : IProducerMiddleware
        {
            _middlewares.Add(typeof(TMiddleware));
            return this;
        }

        public ServiceBusProducerConfigurationBuilder<TMessage> WithTopic(string topic)
        {
            _topicName = topic;
            return this;
        }

        public ServiceBusProducerConfigurationBuilder<TMessage> EnsureQueueExists(string queueName)
        {
            var managementClient = new ManagementClient(_azureServiceBusConfiguration.ConnectionString);

            if (!managementClient.QueueExistsAsync(queueName).GetAwaiter().GetResult())
            {
                managementClient.CreateQueueAsync(queueName).GetAwaiter().GetResult();
            }

            managementClient.CloseAsync().GetAwaiter().GetResult();

            return this;
        }

        public ServiceBusProducerConfigurationBuilder<TMessage> EnsureTopicExists(string topicName)
        {
            var managementClient = new ManagementClient(_azureServiceBusConfiguration.ConnectionString);
            if (!managementClient.TopicExistsAsync(topicName).GetAwaiter().GetResult())
            {
                managementClient.CreateTopicAsync(topicName).GetAwaiter().GetResult();
            }

            managementClient.CloseAsync().GetAwaiter().GetResult();
            return this;
        }

        public ServiceBusProducerConfigurationBuilder<TMessage> WithCommandProducer()
        {
            _services.AddScoped(typeof(ICommandProducer<>), typeof(CommandProducer<>));
            return this;
        }

        public ServiceBusProducerConfigurationBuilder<TMessage> WithEventProducer()
        {
            _services.AddScoped(typeof(IEventProducer<>), typeof(EventProducer<>));
            return this;
        }

        public ServiceBusProducerConfigurationBuilder<TMessage> ToTopic(string topicName)
        {
            _topicName = topicName;
            return this;
        }

        public ServiceBusProducerConfigurationBuilder<TMessage> ToQueue(string queueName)
        {
            _queueName = queueName;
            return this;
        }

        public ServiceBusProducerConfigurationBuilder<TMessage> WithQueue(string queue)
        {
            _queueName = queue;
            return this;
        }

        internal void Build()
        {
            if (string.IsNullOrEmpty(_queueName) && string.IsNullOrEmpty(_topicName))
                throw new InvalidOperationException("Either topic or queue name must be specified.");

            foreach (var middlewareType in _middlewares)
            {
                _services.AddScoped(typeof(IProducerMiddleware), middlewareType);
            }

            _services.AddScoped<IServiceBusProducer<TMessage>>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ServiceBusProducer<TMessage>>>();
                var middlewares = sp.GetServices<IProducerMiddleware>();
                var name = _queueName ?? _topicName!;
                return new ServiceBusProducer<TMessage>(
                    _azureServiceBusConfiguration,
                    name,
                    logger,
                    middlewares);
            });
        }
    }

}
