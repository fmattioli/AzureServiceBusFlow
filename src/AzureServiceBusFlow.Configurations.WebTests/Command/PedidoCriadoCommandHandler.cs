using Azure.Messaging.ServiceBus;

using AzureServiceBusFlow.Abstractions;

namespace AzureServiceBusFlow.Configurations.WebTests.Command
{
    public class PedidoCriadoHandler : IMessageHandler<PedidoCriadoCommand>
    {
        public Task HandleAsync(PedidoCriadoCommand message, ServiceBusReceivedMessage rawMessage)
        {
            Console.WriteLine($"[PedidoCriadoHandler] Pedido {1} para {"JAO"} no valor de {50}");
            return Task.CompletedTask;
        }
    }
}
