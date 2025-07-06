using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace AzureServiceBusFlow.Hosts
{
    public class ServiceBusQueueConsumerHostedService<TMessage>(
        string connectionString,
        string queueName,
        Func<TMessage, IServiceProvider, Task> messageHandler,
        IServiceProvider serviceProvider,
        ILogger _logger)
       : IHostedService, IAsyncDisposable
    {
        private readonly ServiceBusClient _client = new(connectionString);
        private ServiceBusProcessor _processor = null!;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions
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
                var message = JsonConvert.DeserializeObject<TMessage>(body);

                if (message != null!)
                {
                    await messageHandler(message, serviceProvider);
                    await args.CompleteMessageAsync(args.Message);
                }
                else
                {
                    await args.DeadLetterMessageAsync(args.Message, "Deserialization failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Process message error: {ErrorSource}", ex.StackTrace);
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private Task ProcessErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Processor error: {ErrorSource}", args.ErrorSource);
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
                await _processor.DisposeAsync();

            await _client.DisposeAsync();

            GC.SuppressFinalize(this);
        }
    }
}
