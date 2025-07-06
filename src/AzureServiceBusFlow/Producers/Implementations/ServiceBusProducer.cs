using Azure.Messaging.ServiceBus;
using AzureServiceBusFlow.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureServiceBusFlow.Producers.Implementations
{
    public class ServiceBusProducer<TMessage> : IServiceBusProducer<TMessage> where TMessage : class, IServiceBusMessage
    {
        private readonly ServiceBusSender _sender;
        private readonly ILogger _logger;
        public ServiceBusProducer(string connectionString, string queueOrTopicName, ILogger logger)
        {
            var client = new ServiceBusClient(connectionString);
            _sender = client.CreateSender(queueOrTopicName);

            _logger = logger;
        }

        public async Task ProduceAsync(TMessage message, CancellationToken cancellationToken)
        {
            var json = JsonConvert.SerializeObject(message);
            var serviceBusMessage = new ServiceBusMessage(json)
            {
                Subject = message.RoutingKey,
                ApplicationProperties =
                {
                    { "MessageType", message.GetType().Name },
                    { "CreatedAt", message.CommandCreatedDate.ToString("O") }
                }
            };

            await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);

            _logger.LogInformation("Message published with successfully!");
        }
    }
}
