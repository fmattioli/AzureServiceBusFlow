using AzureServiceBusFlow.Models;

namespace AzureServiceBusFlow.Abstractions;

public interface ICommandProducer<in TCommand> where TCommand : class, IServiceBusMessage
{
    /// <summary>
    /// Produces the command without additional options.
    /// </summary>
    Task ProduceCommandAsync(TCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Produces the command using explicit message options such as delay or application properties.
    /// </summary>
    Task ProduceCommandAsync(TCommand command, MessageOptions messageOptions, CancellationToken cancellationToken);

    /// <summary>
    /// Produces the command with a set of application properties that will be attached to the message.
    /// </summary>
    Task ProduceCommandAsync(TCommand command, IDictionary<string, object> applicationProperties, CancellationToken cancellationToken);

    /// <summary>
    /// Produces the command with a delivery delay before it becomes available for processing.
    /// </summary>
    Task ProduceCommandAsync(TCommand command, TimeSpan delay, CancellationToken cancellationToken);
}
