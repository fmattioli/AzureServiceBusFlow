using AzureServiceBusFlow.Configurations.Abstractions;

namespace AzureServiceBusFlow.Configurations.WebTests.Command
{
    public class OutroHandlerQualquerCommandHandler : IMessageHandler<PedidoCriadoCommand>
    {
        public Task HandleAsync(PedidoCriadoCommand message, CancellationToken cancellationToken)
        {
            Console.WriteLine($"[PedidoCriadoHandler] Pedido {1} para {"JAO"} no valor de {50}");
            return Task.CompletedTask;
        }
    }
}
