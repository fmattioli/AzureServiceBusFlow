using Azure.Messaging.ServiceBus;

namespace AzureServiceBusFlow.Abstractions
{
    public interface IMessageHandler<in T>
    {
        Task HandleAsync(T message, ServiceBusReceivedMessage rawMessage, CancellationToken cancellationToken);
    }
}
