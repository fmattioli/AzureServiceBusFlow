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
                    { "MessageType", message.GetType().FullName },
                    { "CreatedAt", message.CreatedDate.ToString("O") }
                }
            };

            await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);

            _logger.LogInformation("Message {MessageType} published with successfully!", message.GetType().Name);
        }

        public Task ProduceAsync(TMessage message, MessageOptions producerOptions, CancellationToken cancellationToken)
        {
            var json = JsonConvert.SerializeObject(message);
            var serviceBusMessage = new ServiceBusMessage(json)
            {
                Subject = (message as IServiceBusMessage)?.RoutingKey ?? message.GetType().Name,
                ApplicationProperties =
                {
                    { "MessageType", message.GetType().FullName },
                    { "CreatedAt", (message as IServiceBusMessage)?.CreatedDate.ToString("O") ?? DateTime.UtcNow.ToString("O") }
                }
            };

            if (producerOptions?.ApplicationProperties is not null)
            {
                producerOptions?.ApplicationProperties?
                    .Where(kvp => !serviceBusMessage.ApplicationProperties.ContainsKey(kvp.Key))
                    .ToList()
                    .ForEach(kvp => serviceBusMessage.ApplicationProperties.Add(kvp.Key, kvp.Value));
            }

            if (producerOptions?.Delay is not null)
            {
                serviceBusMessage.ScheduledEnqueueTime = DateTimeOffset.UtcNow.Add(producerOptions.Delay.Value);
            }

            return _sender.SendMessageAsync(serviceBusMessage, cancellationToken);
        }

        public Task ProduceAsync(TMessage message, TimeSpan delay, CancellationToken cancellationToken)
        {
            return ProduceAsync(message, new MessageOptions(delay, null), cancellationToken);
        }

        public Task ProduceAsync(TMessage message, IDictionary<string, object> applicationProperties, CancellationToken cancellationToken)
        {
            return ProduceAsync(message, new MessageOptions(null, applicationProperties), cancellationToken);
        }
    }
}
