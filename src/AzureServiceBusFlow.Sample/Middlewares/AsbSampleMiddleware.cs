using Azure.Messaging.ServiceBus;
using AzureServiceBusFlow.Middlewares;

namespace AzureServiceBusFlow.Sample.Middlewares
{
    /// <summary>
    /// Sample middleware that adds a custom property
    /// to every message published to Azure Service Bus.
    /// </summary>
    public class AsbSampleMiddleware : IProducerMiddleware
    {
        public async Task InvokeAsync(ServiceBusMessage message, Func<Task> next)
        {
            // Add a custom property if it doesn't exist
            if (!message.ApplicationProperties.ContainsKey("SampleMiddleware"))
            {
                message.ApplicationProperties["SampleMiddleware"] = "Executed";
            }

            // Add a UTC timestamp for debugging or tracking
            message.ApplicationProperties["ProcessedAtUtc"] = DateTime.UtcNow.ToString("O");

            // Continue the pipeline (invoke the next middleware or the actual send)
            await next();
        }
    }
}
