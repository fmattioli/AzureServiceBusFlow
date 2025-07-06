using Azure.Messaging.ServiceBus;

using AzureServiceBusFlow.Abstractions;

namespace AzureServiceBusFlow.Configurations.WebTests.Command
{
    public class PedidoRecebidoCommandHandler : IMessageHandler<PedidoRecebidoCommand>
    {
        public Task HandleAsync(PedidoRecebidoCommand message, ServiceBusReceivedMessage rawMessage)
        {
            Console.WriteLine($"[PedidoRecebidoCommandHandler] Pedido {1} para {"JAO"} no valor de {50}");
            return Task.CompletedTask;
        }
    }
}
