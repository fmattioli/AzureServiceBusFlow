using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Configurations.ApiTestTwo.Command;
using System.Runtime.Serialization;

namespace AzureServiceBusFlow.Configurations.ApiTestTwo.CommandHandler
{
    public class PedidoCriadoCommand(PedidoCriado pedidoCriado) : IServiceBusMessage
    {
        [IgnoreDataMember]
        public string RoutingKey { get; set; } = pedidoCriado.Id.ToString();

        [IgnoreDataMember]
        public DateTime MessageCreatedDate { get; set; } = DateTime.UtcNow;

        [DataMember(Order = 1)]
        public PedidoCriado Category { get; set; } = pedidoCriado;
    }
}
