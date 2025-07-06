namespace AzureServiceBusFlow.Configurations.Abstractions
{
    public interface IServiceBusMessage
    {
        string RoutingKey { get; }
        DateTime CommandCreatedDate { get; }
    }
}
