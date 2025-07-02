using AzureServiceBusFlow.Configurations.Abstractions;

namespace AzureServiceBusFlow.Configurations.WebTests.Config
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
