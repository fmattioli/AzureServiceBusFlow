# ⚙️ **Message Handlers**

A **Message Handler** defines how a message should be processed once it is received from Azure Service Bus.  
In **AzureServiceBusFlow**, every handler must implement the `IMessageHandler<TMessage>` interface, where `TMessage` is the type of message being handled.

This ensures a clean and consistent processing model across all services.

<br>

## 🧩 IMessageHandler Interface

```csharp
public interface IMessageHandler<in TMessage>
{
    Task HandleAsync(
        TMessage message,
        ServiceBusReceivedMessage rawMessage,
        CancellationToken cancellationToken);
}
```
<br>

## 🧱 Example of a Command Handler

```csharp
public class CommandExample1Handler : IMessageHandler<ExampleCommand1>
{
    public Task HandleAsync(ExampleCommand1 message, ServiceBusReceivedMessage rawMessage, CancellationToken cancellationToken)
    {
        // Business logic to handle the command
        Console.WriteLine($"Processing command for client: {message.ExampleMessage.Cliente}");

        return Task.CompletedTask;
    }
}
```

In this example:
- The handler consumes an **`ExampleCommand1`** message.
- It receives both the **deserialized message** and the raw **Azure Service Bus message**.
- The method **`HandleAsync`** is executed automatically when a message is consumed.

<br>

## 🧠 Key Benefits

- **Strong typing** ensures compile-time safety.
- **Clear separation** between different message types and business logic.
- **Automatic binding** via dependency injection and the AsbFlow configuration.

Handlers allow your application to stay modular, scalable, and easy to test.
