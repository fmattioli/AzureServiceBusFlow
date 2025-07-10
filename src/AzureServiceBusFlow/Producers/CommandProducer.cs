using AzureServiceBusFlow.Abstractions;

namespace AzureServiceBusFlow.Producers
{
    public class CommandProducer<TCommand>(IServiceBusProducer<TCommand> producer) : ICommandProducer<TCommand>
    where TCommand : class, IServiceBusMessage
    {
        private readonly IServiceBusProducer<TCommand> _producer = producer;

        public Task ProduceCommandAsync(TCommand command, CancellationToken cancellationToken)
        {
            return _producer.ProduceAsync(command, cancellationToken);
        }
    }
}
