using AzureServiceBusFlow.Abstractions;

namespace AzureServiceBusFlow.Producers.Abstractions
{
    public interface ICommandProducer
    {
        Task ProduceCommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : IServiceBusMessage;
    }
}
