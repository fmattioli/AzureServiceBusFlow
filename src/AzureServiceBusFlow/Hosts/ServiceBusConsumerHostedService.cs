using Azure.Messaging.ServiceBus;
using AzureServiceBusFlow.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AzureServiceBusFlow.Hosts;

public class ServiceBusConsumerHostedService(
    Func<ServiceBusReceivedMessage, IServiceProvider, CancellationToken, Task> messageHandler,
    IServiceProvider serviceProvider,
    ILogger logger,
    AzureServiceBusConfiguration azureServiceBusConfiguration,
    string queueOrTopicName,
    string subscriptionName = null!) : IHostedService, IAsyncDisposable
{
    private readonly ServiceBusClient _client = new(azureServiceBusConfiguration.ConnectionString);
    private ServiceBusProcessor _processor = null!;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _processor = subscriptionName is null
            ? _client.CreateProcessor(queueOrTopicName, new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 5,
                MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(azureServiceBusConfiguration.MaxAutoLockRenewalDurationInSeconds),
                AutoCompleteMessages = azureServiceBusConfiguration.ServiceBusReceiveMode == ServiceBusReceiveMode.PeekLock,
                ReceiveMode = azureServiceBusConfiguration.ServiceBusReceiveMode,
                Identifier = queueOrTopicName,
            })
            : _client.CreateProcessor(queueOrTopicName, subscriptionName, new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 5,
                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(60),
                AutoCompleteMessages = false,
                ReceiveMode = azureServiceBusConfiguration.ServiceBusReceiveMode,
                Identifier = queueOrTopicName + " - " + subscriptionName,
            });

        _processor.ProcessMessageAsync += ProcessMessageHandler;
        _processor.ProcessErrorAsync += ProcessErrorHandler;

        return _processor.StartProcessingAsync(cancellationToken);
    }

    private async Task ProcessMessageHandler(ProcessMessageEventArgs args)
    {
        await messageHandler(args.Message, serviceProvider, args.CancellationToken);
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
