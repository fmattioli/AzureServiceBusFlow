using AzureServiceBusFlow.Models;

namespace AzureServiceBusFlow.Abstractions;

public interface ICommandProducer<in TCommand> where TCommand : class, IServiceBusMessage
{
    Task ProduceCommandAsync(TCommand command, CancellationToken cancellationToken);

    Task ProduceCommandAsync(TCommand command, MessageOptions messageOptions, CancellationToken cancellationToken);
}
