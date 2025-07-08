using Azure.Messaging.ServiceBus;
using AzureServiceBusFlow.Abstractions;

namespace AzureServiceBusFlow.Configurations.ApiTestOne.Command
{
    public class PedidoRecebidoCommandHandler : IMessageHandler<PedidoCriadoCommand>
    {
        public Task HandleAsync(PedidoCriadoCommand message, ServiceBusReceivedMessage rawMessage)
        {
            Console.WriteLine($"[PedidoRecebidoCommandHandler - 1] Pedido {1} para {"JAO"} no valor de {50}");
            return Task.CompletedTask;
        }
    }
}
