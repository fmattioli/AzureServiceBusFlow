using Azure.Messaging.ServiceBus;
using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Models;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureServiceBusFlow.Producers
{
    public class ServiceBusProducer<TMessage> : IServiceBusProducer<TMessage> where TMessage : class, IServiceBusMessage
    {
        private readonly ServiceBusSender _sender;
        private readonly ILogger _logger;
        public ServiceBusProducer(AzureServiceBusConfiguration azureServiceBusConfiguration, string queueOrTopicName, ILogger logger)
        {
            var client = new ServiceBusClient(azureServiceBusConfiguration.ConnectionString);
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
                    { "CreatedAt", message.CreatedDate.ToString("O") }
                }
            };

            await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);

            _logger.LogInformation("Message {MessageType} published with successfully!", message.GetType().Name);
        }
    }
}
