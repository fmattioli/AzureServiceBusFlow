using AzureServiceBusFlow.Configurations.Builders;
using AzureServiceBusFlow.Configurations.Producers.Abstractions;
using AzureServiceBusFlow.Configurations.Producers.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace AzureServiceBusFlow.Configurations.Extensions
{
    public static class AzureServiceBusExtensions
    {
        public static IServiceCollection AddAzureServiceBus(this IServiceCollection services, Action<ServiceBusConfigurationBuilder> configure)
        {
            var builder = new ServiceBusConfigurationBuilder(services);

            configure(builder);

            builder.Build();

            return services;
        }

        public static IServiceCollection WithCommandProducer(this IServiceCollection services)
        {
            services.AddSingleton<ICommandProducer, CommandProducer>();
            return services;
        }
    }
}
