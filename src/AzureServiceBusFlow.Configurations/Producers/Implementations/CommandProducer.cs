using AzureServiceBusFlow.Configurations.Abstractions;
using AzureServiceBusFlow.Configurations.Producers.Abstractions;
using Microsoft.Extensions.Logging;

namespace AzureServiceBusFlow.Configurations.Producers.Implementations
{
    public class CommandProducer(
        IServiceBusProducer<IServiceBusMessage> producer,
        ILogger<CommandProducer> logger) : ICommandProducer
    {
        public async Task ProduceCommandAsync<TCommand>(TCommand command) where TCommand : IServiceBusMessage
        {
            await producer.ProduceAsync(command);
            logger.LogInformation("Command produced: {@Command}", command);
        }
    }
}
