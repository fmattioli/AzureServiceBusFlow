using AzureServiceBusFlow.Abstractions;
using Microsoft.Extensions.Logging;

namespace AzureServiceBusFlow.Producers
{
    public class EventProducer(
        IServiceBusProducer<IServiceBusMessage> producer,
        ILogger<CommandProducer> logger) : IEventProducer
    {
        public async Task ProduceEventAsync<TEvent>(TEvent command, CancellationToken cancellationToken) where TEvent : IServiceBusMessage
        {
            await producer.ProduceAsync(command, cancellationToken);
            logger.LogInformation("Event produced: {@Event}", command);
        }
    }
}
