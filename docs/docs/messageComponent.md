# Messages

In **AzureServiceBusFlow (AsbFlow)**, every message that is sent through the Azure Service Bus — whether it is a **Command** or an **Event** — must implement the `IServiceBusMessage` interface.

This interface defines the basic structure that all messages must follow, ensuring consistency and allowing the framework to automatically handle serialization, metadata, and routing.

<br>

## 🧩 IServiceBusMessage Interface

```csharp
public interface IServiceBusMessage
{
    string RoutingKey { get; }
    DateTime CreatedDate { get; }
}
```

Every message must expose:
- **RoutingKey** → used to identify or categorize the message (often a unique ID or business key)
- **CreatedDate** → timestamp of when the message was created

<br>

## 🧱 Example of a Command Message

```csharp
public class ExampleCommand1 : IServiceBusMessage
{
    public string RoutingKey => ExampleMessage.Id.ToString();
    public DateTime CreatedDate => DateTime.UtcNow;
    public required ExampleMessage ExampleMessage { get; set; }
}

public class ExampleMessage
{
    public Guid Id { get; set; }
    public string? Cliente { get; set; }
    public decimal Valor { get; set; }
}
```

This structure provides:
- A **strongly typed** message model
- Easy serialization through the built-in producer
- A unified standard for both **commands** and **events**

<br>

## ⚙️ Architectural Meaning

Although both **commands** and **events** share the same implementation, they differ in intention:
- A **Command** represents the intention to perform an action.
- An **Event** indicates that something has already happened.

The distinction is purely architectural and helps maintain clear separation of responsibilities between services.
