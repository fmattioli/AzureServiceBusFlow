using AzureServiceBusFlow.Abstractions;

namespace AzureServiceBusFlow.Producers
{
    public class EventProducer<TEvent>(IServiceBusProducer<TEvent> producer) : IEventProducer<TEvent> where TEvent : class, IServiceBusMessage
    {
        private readonly IServiceBusProducer<TEvent> _producer = producer;

        public Task ProduceEventAsync(TEvent @event, CancellationToken cancellationToken)
        {
            return _producer.ProduceAsync(@event, cancellationToken);
        }
    }
}
