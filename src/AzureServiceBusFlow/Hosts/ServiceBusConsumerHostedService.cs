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
        var options = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = azureServiceBusConfiguration.MaxConcurrentCalls,
            MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(azureServiceBusConfiguration.MaxAutoLockRenewalDurationInSeconds),
            AutoCompleteMessages = false,
            ReceiveMode = azureServiceBusConfiguration.ServiceBusReceiveMode,
            Identifier = queueOrTopicName
        };

        _processor = subscriptionName is null
            ? _client.CreateProcessor(queueOrTopicName, options)
            : _client.CreateProcessor(queueOrTopicName, subscriptionName, options);

        _processor.ProcessMessageAsync += ProcessMessageHandler;
        _processor.ProcessErrorAsync += ProcessErrorHandler;

        return _processor.StartProcessingAsync(cancellationToken);
    }

    private async Task ProcessMessageHandler(ProcessMessageEventArgs args)
    {
        var message = args.Message;

        try
        {
            await messageHandler(message, serviceProvider, args.CancellationToken);

            if (_processor.ReceiveMode == ServiceBusReceiveMode.PeekLock)
            {
                await args.CompleteMessageAsync(message, args.CancellationToken);
                logger.LogInformation("Message {MessageId} produced and consumed with successful.", message.MessageId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while trying process MessageId {MessageId} - MessageBody {Body}", message.MessageId, message.Body);

            if (_processor.ReceiveMode == ServiceBusReceiveMode.PeekLock)
            {
                await args.AbandonMessageAsync(message, cancellationToken: args.CancellationToken);
                logger.LogInformation("Message {MessageId} abandoned. Will retry again.", message.MessageId);
            }
        }
    }

    private Task ProcessErrorHandler(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception, "Erro no processor: {ErrorSource}", args.ErrorSource);
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
