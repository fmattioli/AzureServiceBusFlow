namespace AzureServiceBusFlow.Sample.Models
{
    public class ExampleMessage
    {
        public Guid Id { get; set; }
        public string? Cliente { get; set; }
        public decimal Valor { get; set; }
    }
}
