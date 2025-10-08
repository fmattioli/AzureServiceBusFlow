using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Sample.Models;

namespace AzureServiceBusFlow.Sample.Commands
{

    public class ExampleCommand1 : IServiceBusMessage
    {
        public string RoutingKey => ExampleMessage.Id.ToString();
        public DateTime CreatedDate => DateTime.UtcNow;
        public required ExampleMessage ExampleMessage { get; set; }
    }

    public class ExampleCommand2 : IServiceBusMessage
    {
        public string RoutingKey => ExampleMessage.Id.ToString();
        public DateTime CreatedDate => DateTime.UtcNow;
        public required ExampleMessage ExampleMessage { get; set; }
    }
}
