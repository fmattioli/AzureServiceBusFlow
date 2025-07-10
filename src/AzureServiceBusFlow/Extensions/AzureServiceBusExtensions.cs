using AzureServiceBusFlow.Builders;
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
    }
}
