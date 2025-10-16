using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Models;

namespace AzureServiceBusFlow.Producers
{
    /// <summary>
    /// A producer responsible for dispatching command messages through the underlying Service Bus producer.
    /// </summary>
    /// <typeparam name="TCommand">The type of command being produced.</typeparam>
    public class CommandProducer<TCommand>(IServiceBusProducer<TCommand> producer) : ICommandProducer<TCommand>
        where TCommand : class, IServiceBusMessage
    {
        private readonly IServiceBusProducer<TCommand> _producer = producer;

        /// <inheritdoc />
        public Task ProduceCommandAsync(TCommand command, CancellationToken cancellationToken)
        {
            return _producer.ProduceAsync(command, cancellationToken);
        }

        /// <inheritdoc />
        public Task ProduceCommandAsync(TCommand command, MessageOptions messageOptions, CancellationToken cancellationToken)
        {
            return _producer.ProduceAsync(command, messageOptions, cancellationToken);
        }

        /// <summary>
        /// Produces a command with a delivery delay.
        /// </summary>
        public Task ProduceCommandAsync(TCommand command, TimeSpan delay, CancellationToken cancellationToken)
        {
            return _producer.ProduceAsync(command, delay, cancellationToken);
        }

        /// <summary>
        /// Produces a command with application properties.
        /// </summary>
        public Task ProduceCommandAsync(TCommand command, IDictionary<string, string> applicationProperties, CancellationToken cancellationToken)
        {
            return _producer.ProduceAsync(command, applicationProperties, cancellationToken);
        }
    }
}
