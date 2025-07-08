using AzureServiceBusFlow.Abstractions;

namespace AzureServiceBusFlow.Configurations.ApiTestTwo.Command
{
    public class ExampleMessage
    {
        public Guid Id { get; set; }
        public string? Cliente { get; set; }
        public decimal Valor { get; set; }
    }

    public class ExampleCommand1 : IServiceBusMessage
    {
        public string RoutingKey => Category.Id.ToString();
        public DateTime MessageCreatedDate => DateTime.UtcNow;
        public required ExampleMessage Category { get; set; }
    }

    public class ExampleCommand2 : IServiceBusMessage
    {
        public required string RoutingKey { get; set; }
        public required DateTime MessageCreatedDate { get; set; }
        public required string Name { get; set; }
    }

    public class ExampleCommand3 : IServiceBusMessage
    {
        public required string RoutingKey { get; set; }
        public required DateTime MessageCreatedDate { get; set; }
        public required string Name { get; set; }
    }
}
