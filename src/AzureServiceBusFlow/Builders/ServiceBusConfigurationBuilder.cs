using AzureServiceBusFlow.Abstractions;
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

        public void Build()
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new InvalidOperationException("Connection string is required.");
            }
        }
    }
}
