using AzureServiceBusFlow.Abstractions;

namespace AzureServiceBusFlow.Producers
{
    public class EventProducer(IServiceBusProducer<IServiceBusMessage> producer) : IEventProducer
    {
        public async Task ProduceEventAsync<TEvent>(TEvent eventBody, CancellationToken cancellationToken) where TEvent : IServiceBusMessage
        {
            await producer.ProduceAsync(eventBody, cancellationToken);
        }
    }
}
