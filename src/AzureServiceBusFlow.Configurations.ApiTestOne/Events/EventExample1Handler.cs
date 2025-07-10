using Azure.Messaging.ServiceBus;
using AzureServiceBusFlow.Abstractions;

namespace AzureServiceBusFlow.Sample.Queues.Events
{
    public class EventExample1Handler : IMessageHandler<ExampleEvent1>
    {
        public Task HandleAsync(ExampleEvent1 message, ServiceBusReceivedMessage rawMessage)
        {
            return Task.CompletedTask;
        }
    }

    public class EventExampleTwoHandlersPerOneMessageHandler : IMessageHandler<ExampleEvent1>
    {
        public Task HandleAsync(ExampleEvent1 message, ServiceBusReceivedMessage rawMessage)
        {
            return Task.CompletedTask;
        }
    }

    public class EventExample2Handler : IMessageHandler<ExampleEvent2>
    {
        public Task HandleAsync(ExampleEvent2 message, ServiceBusReceivedMessage rawMessage)
        {
            return Task.CompletedTask;
        }
    }
}
