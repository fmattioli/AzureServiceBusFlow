namespace AzureServiceBusFlow.Configurations.Abstractions
{
    public interface IMessageHandler<in T>
    {
        Task HandleAsync(T message, CancellationToken cancellationToken = default);
    }
}
