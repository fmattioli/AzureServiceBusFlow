using Azure.Messaging.ServiceBus;
using AzureServiceBusFlow.Configurations.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureServiceBusFlow.Configurations.Producers
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

        public async Task ProduceAsync(TMessage message)
        {
            var json = JsonConvert.SerializeObject(message);
            var serviceBusMessage = new ServiceBusMessage(json)
            {
                Subject = message.RoutingKey,
                ApplicationProperties =
                {
                    { "MessageType", typeof(TMessage).Name },
                    { "CreatedAt", message.CommandCreatedDate.ToString("O") }
                }
            };

            await _sender.SendMessageAsync(serviceBusMessage);

            _logger.LogInformation("Message published with successfully!");
        }
    }
}
