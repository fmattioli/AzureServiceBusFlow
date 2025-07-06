using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Producers.Abstractions;
using AzureServiceBusFlow.Producers.Implementations;

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

        public ServiceBusProducerConfigurationBuilder<TMessage> WithCommandProducer()
        {
            _services.AddSingleton<ICommandProducer, CommandProducer>();
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
