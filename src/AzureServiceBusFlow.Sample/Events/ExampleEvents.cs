using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Sample.Models;

namespace AzureServiceBusFlow.Sample.Events
{
    public class ExampleEvent1 : IServiceBusMessage
    {
        public string RoutingKey => ExampleMessage.Id.ToString();
        public DateTime CreatedDate => DateTime.UtcNow;
        public required ExampleMessage ExampleMessage { get; set; }
    }

    public class ExampleEvent2 : IServiceBusMessage
    {
        public string RoutingKey => ExampleMessage.Id.ToString();
        public DateTime CreatedDate => DateTime.UtcNow;
        public required ExampleMessage ExampleMessage { get; set; }
    }

    public class ExampleEvent3 : IServiceBusMessage
    {
        public string RoutingKey => ExampleMessage.Id.ToString();
        public DateTime CreatedDate => DateTime.UtcNow;
        public required ExampleMessage ExampleMessage { get; set; }
    }
}
