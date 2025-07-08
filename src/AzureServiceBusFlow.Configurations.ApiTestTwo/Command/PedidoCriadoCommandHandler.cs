using Azure.Messaging.ServiceBus;
using AzureServiceBusFlow.Abstractions;

namespace AzureServiceBusFlow.Configurations.ApiTestTwo.Command
{
    public class PedidoCriadoHandler : IMessageHandler<PedidoCriadoCommand>
    {
        public Task HandleAsync(PedidoCriadoCommand message, ServiceBusReceivedMessage rawMessage)
        {
            Console.WriteLine($"[PedidoCriadoHandler - 2] Pedido {1} para {"JAO"} no valor de {50}");
            return Task.CompletedTask;
        }
    }
}
