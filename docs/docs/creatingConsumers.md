## Consumers

**AzureServiceBusFlow** also simplify the creation of **Consumers** or **MessageHandlers** by implementing the **`IMessageHandler<>`** interface and passing the **Message** that will be consumed as the Type of the **Handler**.

> This interface provides the `HandleAsync()` method that is used to process the received message.

---

### 🛠️ Creating a Consumer

```csharp
public class CommandExample1Handler : IMessageHandler<ExampleCommand1>
{
    public Task HandleAsync(ExampleCommand1 message, ServiceBusReceivedMessage rawMessage, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
```

The MessageHandler above consume a Message of type ExampleCommand1, created earlier in this documentation ([ExampleCommand1](/docs/creatingMessages.html)).

--- 
### ⚙️ Registering Consumer

To register the **Consumer**, aka **MessageHandler**, we need to use the **`AddConsumer()`** extension method in **`AddAzureServiceBus()`** configuration methon in **`Program.cs`**.

> the **`AddConsummer()`** method requires a Action of type ServiceBusConsumerConfigurationBuilder to configure the Consumer using this methods:
> - `FromQueue()`: Defines the Queue this Consumer will be receiving messages.
> - `FromTopic()`: Defines the Topic this Consumer will be receiving messages.
> - `EnsureSubscriptionExists()`: Ensure that the subscription passed exists in AzureServiceBus, creates it if doesn`t exists.
> - `AddHandler<TMessage, TMessageHandler>()`: Defines the **Consumer** / **MessageHandler** that is goint to consume a specific Message from a Queue or a Topic. More than one Consumer to the same **Message**, **Queue** or **Topic** can be defined in the same `AddConsumer()`

This example shows the full configuration for:
- Registering a **Message**
- Creating a **Producer** for it
- Publishing in a specifi **Queue**
- Configuring 2 **Consumers** / **MessageHandlers** for the same **Message** 
- Consuming this **Message** from the same **Queue**.

```csharp
builder.Services.AddAzureServiceBus(cfg => cfg
    .ConfigureAzureServiceBus(azureServiceBusConfig)
    .AddProducer<ExampleCommand1>(p => p
        .EnsureQueueExists("command-queue-one")
        .WithCommandProducer()
        .ToQueue("command-queue-one"))
    .AddConsumer(c => c
        .FromQueue("command-queue-one")
        .AddHandler<ExampleCommand1, CommandExemple1Handler>()
        .AddHandler<ExampleCommand1, CommandExampleTwoHandlersPerOneMessageHandler>())
    );
```