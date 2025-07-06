namespace AzureServiceBusFlow.Configurations.Abstractions
{
    public interface IServiceBusProducer<in TMessage> where TMessage : class, IServiceBusMessage
    {
        Task ProduceAsync(TMessage message);
    }
}
