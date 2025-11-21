using Azure.Messaging.ServiceBus;
using AzureServiceBusFlow.Middlewares;

namespace AzureServiceBusFlow.Sample.Middlewares
{
    /// <summary>
    /// Sample middleware that log the ApplicationProperties
    /// before the message reach the MessageHandler
    /// </summary>
    public class AsbSampleConsumerMiddleware(ILogger<AsbSampleConsumerMiddleware> logger) : IConsumerMiddleware
    {
        public Task InvokeAsync(ServiceBusReceivedMessage message, Func<Task> next, CancellationToken cancellationToken)
        {
            var timeInUtc = message.ApplicationProperties["ProcessedAtUtc"];

            logger.LogInformation("Message reached ConsumerMiddleware === ProcessedAt: {TimeInUtc}", timeInUtc);

            return next();
        }
    }
}
