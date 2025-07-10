namespace AzureServiceBusFlow.Abstractions
{
    public interface IServiceBusMessage
    {
        string RoutingKey { get; }
        DateTime CreatedDate { get; }
    }
}
