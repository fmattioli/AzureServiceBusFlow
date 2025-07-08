using Azure.Messaging.ServiceBus;
using AzureServiceBusFlow.Abstractions;

namespace AzureServiceBusFlow.Configurations.ApiTestOne.Command
{
    public class PedidoCriadoHandler : IMessageHandler<ExampleCommand1>
    {
        public Task HandleAsync(ExampleCommand1 message, ServiceBusReceivedMessage rawMessage)
        {
            return Task.CompletedTask;
        }
    }
}
