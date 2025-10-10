using AzureServiceBusFlow.Models;

namespace AzureServiceBusFlow.Abstractions
{
    public interface IServiceBusProducer<in TMessage> where TMessage : class, IServiceBusMessage
    {
        Task ProduceAsync(TMessage message, CancellationToken cancellationToken);

        Task ProduceAsync(
            TMessage message,
            MessageOptions producerOptions,
            CancellationToken cancellationToken);
    }
}
