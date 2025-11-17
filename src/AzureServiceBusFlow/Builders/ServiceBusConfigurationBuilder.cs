using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Middlewares;
using AzureServiceBusFlow.Models;

using Microsoft.Extensions.DependencyInjection;

namespace AzureServiceBusFlow.Builders
{
    /// <summary>
    /// Builder for configuring Azure Service Bus, allowing connection, producers, and consumers setup.
    /// </summary>
    public class ServiceBusConfigurationBuilder(IServiceCollection services)
    {
        private readonly IServiceCollection _services = services;

        /// <summary>
        /// The connection string used to connect to Azure Service Bus.
        /// </summary>
        public AzureServiceBusConfiguration AzureServiceBusConfiguration { get; private set; } = null!;

        /// <summary>
        /// Sets the connection string that will be used for all Service Bus operations.
        /// </summary>
        /// <param name="connectionString">Azure Service Bus connection string.</param>
        /// <returns>Returns the builder itself for method chaining.</returns>
        public ServiceBusConfigurationBuilder ConfigureAzureServiceBus(AzureServiceBusConfiguration azureServiceBusConfiguration)
        {
            AzureServiceBusConfiguration = azureServiceBusConfiguration;
            return this;
        }

        public ServiceBusConfigurationBuilder UseGlobalProducerMiddleware<TMiddleware>() 
            where TMiddleware : class, IProducerMiddleware
        {
            // Só registra no DI — já torna global
            if (!_services.Any(s =>
                s.ServiceType == typeof(IProducerMiddleware) &&
                s.ImplementationType == typeof(TMiddleware)))
            {
                _services.AddSingleton<IProducerMiddleware, TMiddleware>();
            }

            return this;
        }

        /// <summary>
        /// Adds a producer for messages of type <typeparamref name="TMessage"/> configured via a callback.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to produce, must implement <see cref="IServiceBusMessage"/>.</typeparam>
        /// <param name="configure">Action to configure the producer, receives a specific builder.</param>
        /// <returns>Returns the builder itself for method chaining.</returns>
        public ServiceBusConfigurationBuilder AddProducer<TMessage>(Action<ServiceBusProducerConfigurationBuilder<TMessage>> configure) where TMessage : class, IServiceBusMessage
        {
            var builder = new ServiceBusProducerConfigurationBuilder<TMessage>(AzureServiceBusConfiguration, _services);
            configure(builder);
            builder.Build();
            return this;
        }

        /// <summary>
        /// Adds a consumer configuration via a callback.
        /// </summary>
        /// <param name="configure">Action to configure the consumer, receives a specific builder.</param>
        /// <returns>Returns the builder itself for method chaining.</returns>
        public ServiceBusConfigurationBuilder AddConsumer(Action<ServiceBusConsumerConfigurationBuilder> configure)
        {
            var builder = new ServiceBusConsumerConfigurationBuilder(AzureServiceBusConfiguration, _services);
            configure(builder);
            builder.Build();
            return this;
        }

        /// <summary>
        /// Validates the configuration and ensures required properties are set.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the connection string is not set.</exception>
        public void Build()
        {
            if (string.IsNullOrEmpty(AzureServiceBusConfiguration.ConnectionString))
            {
                throw new InvalidOperationException("Connection string is required.");
            }
        }
    }
}
