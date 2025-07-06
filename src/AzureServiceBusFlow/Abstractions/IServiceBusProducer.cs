namespace AzureServiceBusFlow.Abstractions
{
    public interface IServiceBusProducer<in TMessage> where TMessage : class, IServiceBusMessage
    {
        Task ProduceAsync(TMessage message, CancellationToken cancellationToken);
    }
}
