using Azure.Messaging.ServiceBus;

namespace AzureServiceBusFlow.Models
{
    public class AzureServiceBusConfiguration
    {
        public required string ConnectionString { get; set; }
        public required ServiceBusReceiveMode ServiceBusReceiveMode { get; set; }
        public int MaxConcurrentCalls { get; set; }
        public int MaxAutoLockRenewalDurationInSeconds { get; set; }
    }
}
