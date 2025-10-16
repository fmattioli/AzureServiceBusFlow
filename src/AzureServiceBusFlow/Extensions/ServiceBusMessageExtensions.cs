using Azure.Messaging.ServiceBus;

namespace AzureServiceBusFlow.Extensions
{
    public static class ServiceBusMessageExtensions
    {
        public static string? GetStringProperty(this ServiceBusReceivedMessage message, string key)
        {
            return message.ApplicationProperties.TryGetValue(key, out var value)
                ? value as string ?? value?.ToString()
                : null;
        }
    }
}
