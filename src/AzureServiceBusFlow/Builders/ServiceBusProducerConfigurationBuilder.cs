using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Producers;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureServiceBusFlow.Builders
{
    public class ServiceBusProducerConfigurationBuilder<TMessage>(string connectionString, IServiceCollection services)
        where TMessage : class, IServiceBusMessage
    {
        private readonly string _connectionString = connectionString;
        private readonly IServiceCollection _services = services;

        private string? _topicName;
        private string? _queueName;

        public ServiceBusProducerConfigurationBuilder<TMessage> WithTopic(string topic)
        {
            _topicName = topic;
            return this;
        }

        public ServiceBusProducerConfigurationBuilder<TMessage> EnsureQueueExists(string queueName)
        {
            var managementClient = new ManagementClient(_connectionString);

            if (!managementClient.QueueExistsAsync(queueName).GetAwaiter().GetResult())
            {
                managementClient.CreateQueueAsync(queueName).GetAwaiter().GetResult();
            }

            managementClient.CloseAsync().GetAwaiter().GetResult();

            return this;
        }

        public ServiceBusProducerConfigurationBuilder<TMessage> EnsureTopicExists(string topicName)
        {
            var managementClient = new ManagementClient(_connectionString);
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
            if (!string.IsNullOrEmpty(_queueName))
            {
                _services.AddSingleton<IServiceBusProducer<TMessage>>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<ServiceBusProducer<TMessage>>>();
                    return new ServiceBusProducer<TMessage>(_connectionString, _queueName, logger);
                });

                return;
            }

            if (!string.IsNullOrEmpty(_topicName))
            {
                _services.AddSingleton<IServiceBusProducer<TMessage>>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<ServiceBusProducer<TMessage>>>();
                    return new ServiceBusProducer<TMessage>(_connectionString, _topicName, logger);
                });

                return;
            }

            throw new InvalidOperationException("Either topic or queue name must be specified.");
        }
    }
}
