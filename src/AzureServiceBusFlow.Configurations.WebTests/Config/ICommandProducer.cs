using AzureServiceBusFlow.Configurations.Abstractions;

namespace AzureServiceBusFlow.Configurations.WebTests.Config
{
    public interface ICommandProducer
    {
        Task ProduceCommandAsync<TCommand>(TCommand command) where TCommand : IServiceBusMessage;
    }
}
