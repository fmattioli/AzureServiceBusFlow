﻿using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AzureServiceBusFlow.Configurations.Hosts
{
    public class ServiceBusTopicConsumerHostedService<TMessage>(
        string connectionString,
        string topicName,
        string subscriptionName,
        Func<TMessage, IServiceProvider, Task> messageHandler,
        IServiceProvider serviceProvider,
        ILogger logger) : IHostedService, IAsyncDisposable
    {
        private readonly ServiceBusClient _client = new(connectionString);
        private ServiceBusProcessor _processor = null!;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _processor = _client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 5,
                AutoCompleteMessages = false,
            });

            _processor.ProcessMessageAsync += ProcessMessageHandler;
            _processor.ProcessErrorAsync += ProcessErrorHandler;

            return _processor.StartProcessingAsync(cancellationToken);
        }

        private async Task ProcessMessageHandler(ProcessMessageEventArgs args)
        {
            try
            {
                var body = args.Message.Body.ToString();
                var message = System.Text.Json.JsonSerializer.Deserialize<TMessage>(body);

                if (message != null)
                {
                    await messageHandler(message, serviceProvider);
                }

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Process message error: {ErrorSource}", ex.StackTrace);
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private Task ProcessErrorHandler(ProcessErrorEventArgs args)
        {
            logger.LogError(args.Exception, "Processor error: {ErrorSource}", args.ErrorSource);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_processor != null)
            {
                await _processor.StopProcessingAsync(cancellationToken);
                await _processor.DisposeAsync();
            }

            await _client.DisposeAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (_processor != null)
            {
                await _processor.DisposeAsync();
            }

            await _client.DisposeAsync();

            GC.SuppressFinalize(this);
        }
    }
}
