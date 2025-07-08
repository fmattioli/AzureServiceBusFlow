namespace AzureServiceBusFlow.Abstractions
{
    public interface IEventProducer
    {
        Task ProduceEventAsync<TEvent>(TEvent command, CancellationToken cancellationToken) where TEvent : IServiceBusMessage;
    }
}
