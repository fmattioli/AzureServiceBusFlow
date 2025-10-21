using Azure.Messaging.ServiceBus;

namespace AzureServiceBusFlow.Middlewar
{
    public interface IProducerMiddleware
    {
        Task InvokeAsync(ServiceBusMessage message, Func<Task> next);
    }

}
