using Azure.Messaging.ServiceBus;
using AzureServiceBusFlow.Middlewares;

namespace AzureServiceBusFlow.Sample.Middlewares
{
    /// <summary>
    /// Sample middleware that log the ApplicationProperties
    /// before the message reach the MessageHandler
    /// </summary>
    public class AsbSampleConsumerMiddleware2(ILogger<AsbSampleConsumerMiddleware2> logger) : IConsumerMiddleware
    {
        public Task InvokeAsync(ServiceBusReceivedMessage message, Func<Task> next, CancellationToken cancellationToken)
        {
            logger.LogInformation("=== Message reached Second ConsumerMiddleware ===");

            return next();
        }
    }
}
