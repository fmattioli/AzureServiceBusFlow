namespace AzureServiceBusFlow.Models
{
    public record MessageOptions(TimeSpan? Delay, IDictionary<string, object>? ApplicationProperties);
}
