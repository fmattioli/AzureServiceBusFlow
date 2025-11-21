using Azure.Messaging.ServiceBus;

namespace AzureServiceBusFlow.Middlewares
{
    public interface IConsumerMiddleware
    {
        Task InvokeAsync(ServiceBusReceivedMessage message, Func<Task> next, CancellationToken cancellationToken);
    }

}
