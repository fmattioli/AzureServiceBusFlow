using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AzureServiceBusFlow.Hosts
{
    public class ServiceBusConsumerHostedService(
        Func<ServiceBusReceivedMessage, IServiceProvider, Task> messageHandler,
        IServiceProvider serviceProvider,
        ILogger logger,
        string connectionString,
        string entityName,
        string subscriptionName = null!) : IHostedService, IAsyncDisposable
    {
        private readonly ServiceBusClient _client = new(connectionString);
        private ServiceBusProcessor _processor = null!;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _processor = subscriptionName is null
                ? _client.CreateProcessor(entityName, new ServiceBusProcessorOptions
                {
                    MaxConcurrentCalls = 5,
                    MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(60),
                    AutoCompleteMessages = false,
                })
                : _client.CreateProcessor(entityName, subscriptionName, new ServiceBusProcessorOptions
                {
                    MaxConcurrentCalls = 5,
                    MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(60),
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
                await messageHandler(args.Message, serviceProvider);
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
