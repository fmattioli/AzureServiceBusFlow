using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Middlewares;
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
            if (!_middlewares.Contains(typeof(TMiddleware)))
            {
                _middlewares.Add(typeof(TMiddleware));
            }

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
            _services.AddSingleton(typeof(ICommandProducer<>), typeof(CommandProducer<>));
            return this;
        }

        public ServiceBusProducerConfigurationBuilder<TMessage> WithEventProducer()
        {
            _services.AddSingleton(typeof(IEventProducer<>), typeof(EventProducer<>));
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
            {
                throw new InvalidOperationException("Either topic or queue name must be specified.");
            }

            foreach (var middlewareType in from middlewareType in _middlewares
                                           where !_services.Any(s =>
                                           s.ServiceType == typeof(IProducerMiddleware) &&
                                           s.ImplementationType == middlewareType)
                                           select middlewareType)
            {
                _services.AddSingleton(typeof(IProducerMiddleware), middlewareType);
            }

            _services.AddSingleton<IServiceBusProducer<TMessage>>(sp =>
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
