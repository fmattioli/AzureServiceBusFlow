using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Builders;
using AzureServiceBusFlow.Producers;
using Microsoft.Extensions.DependencyInjection;

namespace AzureServiceBusFlow.Extensions
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
