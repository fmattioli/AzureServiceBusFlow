using Azure.Messaging.ServiceBus;
using AzureServiceBusFlow.Abstractions;
using AzureServiceBusFlow.Middlewar;
using AzureServiceBusFlow.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureServiceBusFlow.Producers
{
    public class ServiceBusProducer<TMessage> : IServiceBusProducer<TMessage>
        where TMessage : class, IServiceBusMessage
    {
        private readonly ServiceBusSender _sender;
        private readonly ILogger _logger;
        private readonly IEnumerable<IProducerMiddleware>? _middlewares;

        public ServiceBusProducer(
            AzureServiceBusConfiguration azureServiceBusConfiguration,
            string queueOrTopicName,
            ILogger logger,
            IEnumerable<IProducerMiddleware>? middlewares = null)
        {
            var client = new ServiceBusClient(azureServiceBusConfiguration.ConnectionString);
            _sender = client.CreateSender(queueOrTopicName);
            _logger = logger;
            _middlewares = middlewares;
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

            async Task finalStep()
            {
                await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);
                _logger.LogInformation("Message {MessageType} published successfully!", message.GetType().Name);
            }

            // Run middlewares, if it exist
            if (_middlewares != null && _middlewares.Any())
            {
                Func<Task> next = finalStep;
                foreach (var middleware in _middlewares.Reverse())
                {
                    var current = middleware;
                    var prevNext = next;
                    next = () => current.InvokeAsync(serviceBusMessage, prevNext);
                }
                await next();
            }
            else
            {
                await finalStep();
            }
        }

        public async Task ProduceAsync(TMessage message, MessageOptions producerOptions, CancellationToken cancellationToken)
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
                foreach (var kvp in from kvp in producerOptions.ApplicationProperties
                                    where !serviceBusMessage.ApplicationProperties.ContainsKey(kvp.Key)
                                    select kvp)
                {
                    serviceBusMessage.ApplicationProperties.Add(kvp.Key, kvp.Value);
                }
            }

            if (producerOptions?.Delay is not null)
            {
                serviceBusMessage.ScheduledEnqueueTime = DateTimeOffset.UtcNow.Add(producerOptions.Delay.Value);
            }

            // Mesmo pipeline de middlewares para as versões com opções
            Func<Task> finalStep = async () =>
            {
                await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);
                _logger.LogInformation("Message {MessageType} published successfully!", message.GetType().Name);
            };

            if (_middlewares != null && _middlewares.Any())
            {
                Func<Task> next = finalStep;
                foreach (var middleware in _middlewares.Reverse())
                {
                    var current = middleware;
                    var prevNext = next;
                    next = () => current.InvokeAsync(serviceBusMessage, prevNext);
                }
                await next();
            }
            else
            {
                await finalStep();
            }
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
