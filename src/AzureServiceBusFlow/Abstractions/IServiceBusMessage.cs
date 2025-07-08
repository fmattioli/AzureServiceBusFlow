namespace AzureServiceBusFlow.Abstractions
{
    public interface IServiceBusMessage
    {
        string RoutingKey { get; }
        DateTime MessageCreatedDate { get; }
    }
}
