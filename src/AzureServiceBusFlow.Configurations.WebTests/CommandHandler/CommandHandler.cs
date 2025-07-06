using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Configurations.WebTests.Command;

using System.Runtime.Serialization;

namespace AzureServiceBusFlow.Configurations.WebTests.CommandHandler
{
    public class PedidoCriadoCommand(PedidoCriado pedidoCriado) : IServiceBusMessage
    {
        [IgnoreDataMember]
        public string RoutingKey { get; set; } = pedidoCriado.Id.ToString();

        [IgnoreDataMember]
        public DateTime CommandCreatedDate { get; set; } = DateTime.UtcNow;

        [DataMember(Order = 1)]
        public PedidoCriado Category { get; set; } = pedidoCriado;
    }
}
