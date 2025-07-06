using AzureServiceBusFlow.Configurations.Abstractions;

namespace AzureServiceBusFlow.Configurations.Producers.Abstractions
{
    public interface ICommandProducer
    {
        Task ProduceCommandAsync<TCommand>(TCommand command) where TCommand : IServiceBusMessage;
    }
}
