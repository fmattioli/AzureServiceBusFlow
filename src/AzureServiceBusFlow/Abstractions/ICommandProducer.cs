namespace AzureServiceBusFlow.Abstractions;

public interface ICommandProducer<in TCommand> where TCommand : class, IServiceBusMessage
{
    Task ProduceCommandAsync(TCommand command, CancellationToken cancellationToken);
}
