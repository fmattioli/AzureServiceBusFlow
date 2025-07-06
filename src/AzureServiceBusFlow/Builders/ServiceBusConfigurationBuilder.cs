using AzureServiceBusFlow.Abstractions;

using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.DependencyInjection;

namespace AzureServiceBusFlow.Builders
{
    public class ServiceBusConfigurationBuilder(IServiceCollection services)
    {
        private readonly IServiceCollection _services = services;
        public string ConnectionString { get; private set; } = null!;

        public ServiceBusConfigurationBuilder UseConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
            return this;
        }

        public ServiceBusConfigurationBuilder AddProducer<TMessage>(
            Action<ServiceBusProducerConfigurationBuilder<TMessage>> configure)
            where TMessage : class, IServiceBusMessage
        {
            var builder = new ServiceBusProducerConfigurationBuilder<TMessage>(ConnectionString, _services);
            configure(builder);
            builder.Build();
            return this;
        }

        public ServiceBusConfigurationBuilder AddConsumer(Action<ServiceBusConsumerConfigurationBuilder> configure)
        {
            var builder = new ServiceBusConsumerConfigurationBuilder(ConnectionString, _services);
            configure(builder);
            builder.Build();
            return this;
        }

        public ServiceBusConfigurationBuilder EnsureQueueExists(string queueName)
        {
            var managementClient = new ManagementClient(ConnectionString);

            if (!managementClient.QueueExistsAsync(queueName).GetAwaiter().GetResult())
            {
                managementClient.CreateQueueAsync(queueName).GetAwaiter().GetResult();
            }

            managementClient.CloseAsync().GetAwaiter().GetResult();

            return this;
        }

        public ServiceBusConfigurationBuilder EnsureTopicExists(string topicName)
        {
            var managementClient = new ManagementClient(ConnectionString);
            if (!managementClient.TopicExistsAsync(topicName).GetAwaiter().GetResult())
            {
                managementClient.CreateTopicAsync(topicName).GetAwaiter().GetResult();
            }
            managementClient.CloseAsync().GetAwaiter().GetResult();
            return this;
        }

        public ServiceBusConfigurationBuilder EnsureSubscriptionExists(string topicName, string subscriptionName)
        {
            var managementClient = new ManagementClient(ConnectionString);
            if (!managementClient.SubscriptionExistsAsync(topicName, subscriptionName).GetAwaiter().GetResult())
            {
                managementClient.CreateSubscriptionAsync(topicName, subscriptionName).GetAwaiter().GetResult();
            }
            managementClient.CloseAsync().GetAwaiter().GetResult();
            return this;
        }

        public void Build()
        {
            if (string.IsNullOrEmpty(ConnectionString))
                throw new InvalidOperationException("Connection string is required.");
        }
    }
}
