using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Producers.Abstractions;
using Microsoft.Extensions.Logging;

namespace AzureServiceBusFlow.Producers.Implementations
{
    public class CommandProducer(
        IServiceBusProducer<IServiceBusMessage> producer,
        ILogger<CommandProducer> logger) : ICommandProducer
    {
        public async Task ProduceCommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : IServiceBusMessage
        {
            await producer.ProduceAsync(command, cancellationToken);
            logger.LogInformation("Command produced: {@Command}", command);
        }
    }
}
