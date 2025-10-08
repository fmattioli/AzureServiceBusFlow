## Consumers

**AzureServiceBusFlow** also simplify the creation of **Consumers** or **MessageHandlers** by implementing the **`IMessageHandler<>`** interface and passing the **Message** that will be consumed as the Type of the **Handler**.

> This interface provides the `HandleAsync()` method that is used to process the received message.

---

### 🛠️ Creating a Consumer

```csharp
public class CommandExemple1Handler : IMessageHandler<ExampleCommand1>
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

```csharp
.AddConsumer(c => c
    .FromQueue("command-queue-one")
    .AddHandler<ExampleCommand1, CommandExemple1Handler>())
```