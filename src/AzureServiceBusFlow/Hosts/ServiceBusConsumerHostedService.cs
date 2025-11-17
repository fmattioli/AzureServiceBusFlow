using Azure.Messaging.ServiceBus;
using AzureServiceBusFlow.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

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
    private readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
            retryCount: azureServiceBusConfiguration.MaxRetryAttempts,
            sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
            onRetry: (exception, timeSpan, attempt, context) =>
            {
                logger.LogError(exception, "Attempt to process azure service bus message again. Retry {Attempt} after {Delay}", attempt, timeSpan);
            });

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
            if (_processor.ReceiveMode == ServiceBusReceiveMode.ReceiveAndDelete)
            {
                await _retryPolicy.ExecuteAsync(async _ =>
                {
                    await messageHandler(message, serviceProvider, args.CancellationToken);
                }, CancellationToken.None);
            }

            if (_processor.ReceiveMode == ServiceBusReceiveMode.PeekLock)
            {
                await messageHandler(message, serviceProvider, args.CancellationToken);
                await args.CompleteMessageAsync(message, args.CancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while trying process MessageType {MessageType} with RoutingKey {RoutingKey} with id {MessageId} - MessageBody {Body}",
                message.GetType().Name,
                message.Subject, 
                message.MessageId, 
                message.Body);

            if (_processor.ReceiveMode == ServiceBusReceiveMode.PeekLock)
            {
                await args.AbandonMessageAsync(message, cancellationToken: args.CancellationToken);

                logger.LogWarning("Message {MessageType} with RoutingKey {RoutingKey} with id {MessageId} abandoned. Will retry again.",
                    message.GetType().Name,
                    message.Subject,
                    message.MessageId);
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
