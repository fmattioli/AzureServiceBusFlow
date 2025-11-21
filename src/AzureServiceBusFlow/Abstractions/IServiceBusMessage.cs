namespace AzureServiceBusFlow.Abstractions
{
    public interface IServiceBusMessage
    {
        DateTime CreatedDate { get; }
    }
}
