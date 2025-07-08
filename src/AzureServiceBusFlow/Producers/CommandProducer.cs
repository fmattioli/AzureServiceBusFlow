using AzureServiceBusFlow.Abstractions;
using Microsoft.Extensions.Logging;

namespace AzureServiceBusFlow.Producers
{
    public class CommandProducer(
        IServiceBusProducer<IServiceBusMessage> producer) : ICommandProducer
    {
        public async Task ProduceCommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : IServiceBusMessage
        {
            await producer.ProduceAsync(command, cancellationToken);
        }
    }
}
