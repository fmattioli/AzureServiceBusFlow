using AzureServiceBusFlow.Abstractions;

namespace AzureServiceBusFlow.Configurations.WebTests.Command
{
    public class PedidoCriado
    {
        public Guid Id { get; set; }
        public string? Cliente { get; set; }
        public decimal Valor { get; set; }
    }

    public class PedidoCriadoCommand : IServiceBusMessage
    {
        public required string RoutingKey { get; set; }
        public required DateTime CommandCreatedDate { get; set; }
        public required PedidoCriado Category { get; set; }
    }

    public class PedidoRecebidoCommand : IServiceBusMessage
    {
        public required string RoutingKey { get; set; }
        public required DateTime CommandCreatedDate { get; set; }
        public required string Name { get; set; }
    }
}
