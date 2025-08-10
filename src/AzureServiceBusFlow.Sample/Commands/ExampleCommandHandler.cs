using Azure.Messaging.ServiceBus;
using AzureServiceBusFlow.Abstractions;

namespace AzureServiceBusFlow.Sample.Queues.Commands
{
    public class CommandExemple1Handler : IMessageHandler<ExampleCommand1>
    {
        public Task HandleAsync(ExampleCommand1 message, ServiceBusReceivedMessage rawMessage, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class CommandExampleTwoHandlersPerOneMessageHandler : IMessageHandler<ExampleCommand1>
    {
        public Task HandleAsync(ExampleCommand1 message, ServiceBusReceivedMessage rawMessage, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class CommandExample2Handler : IMessageHandler<ExampleCommand2>
    {
        public Task HandleAsync(ExampleCommand2 message, ServiceBusReceivedMessage rawMessage, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
