using Azure.Messaging.ServiceBus;

namespace AzureServiceBusFlow.Middlewares
{
    public interface IProducerMiddleware
    {
        Task InvokeAsync(ServiceBusMessage message, Func<Task> next);
    }

}
