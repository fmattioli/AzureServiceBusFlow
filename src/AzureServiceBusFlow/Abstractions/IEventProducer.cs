namespace AzureServiceBusFlow.Abstractions
{
    public interface IEventProducer
    {
        Task ProduceEventAsync<TEvent>(TEvent eventBody, CancellationToken cancellationToken) where TEvent : IServiceBusMessage;
    }
}
