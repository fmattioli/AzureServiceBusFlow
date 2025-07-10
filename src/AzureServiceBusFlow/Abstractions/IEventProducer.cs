namespace AzureServiceBusFlow.Abstractions
{
    public interface IEventProducer<in TEvent> where TEvent : class, IServiceBusMessage
    {
        Task ProduceEventAsync(TEvent @event, CancellationToken cancellationToken);
    }
}
