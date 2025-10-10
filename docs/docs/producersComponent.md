# **Producers**

**Producers** are responsible for **sending messages** to Azure Service Bus queues or topics.  
The **AzureServiceBusFlow** package provides built-in producer interfaces and implementations, allowing you to send both **Commands** and **Events** easily and consistently.

<br>

## 🧩 Producer Interfaces

The library defines two main producer interfaces:

```csharp
public interface ICommandProducer<in TCommand> where TCommand : class, IServiceBusMessage
{
    Task ProduceCommandAsync(TCommand command, CancellationToken cancellationToken);
}

public interface IEventProducer<in TEvent> where TEvent : class, IServiceBusMessage
{
    Task ProduceEventAsync(TEvent @event, CancellationToken cancellationToken);
}
```

These interfaces abstract the underlying logic of sending messages to the Azure Service Bus.

<br>

## ⚙️ Internal Implementation

Both producers rely on a shared low-level component: the IServiceBusProducer<TMessage> interface.

```csharp
public interface IServiceBusProducer<in TMessage> where TMessage : class, IServiceBusMessage
{
    Task ProduceAsync(TMessage message, CancellationToken cancellationToken);
}
```


The default implementation, **`ServiceBusProducer<TMessage>`**, uses the Azure SDK to serialize and send the message:


```csharp
public class ServiceBusProducer<TMessage> : IServiceBusProducer<TMessage> where TMessage : class, IServiceBusMessage
{
    public async Task ProduceAsync(TMessage message, CancellationToken cancellationToken)
    {
        var json = JsonConvert.SerializeObject(message);
        var serviceBusMessage = new ServiceBusMessage(json)
        {
            Subject = message.RoutingKey,
            ApplicationProperties =
            {
                { "MessageType", message.GetType().FullName },
                { "CreatedAt", message.CreatedDate.ToString("O") }
            }
        };

        await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);
        _logger.LogInformation("Message {MessageType} published successfully!", message.GetType().Name);
    }
}
```
<br>

## 🧱 Example of Usage in a Controller

You don’t need to create a custom producer class — AsbFlow already provides the implementations.
Simply inject the appropriate interface (**`ICommandProducer`** or **`IEventProducer`**) and call the `Produce...Async` method:

```csharp
[Route("api/commands")]
[ApiController]
public class CommandController(ICommandProducer<ExampleCommand1> _producer) : ControllerBase
{
    [HttpPost("command-example-one")]
    public async Task<IActionResult> Example1(CancellationToken cancellationToken)
    {
        ExampleCommand1 command = new()
        {
            ExampleMessage = new ExampleMessage
            {
                Cliente = "jose",
                Id = Guid.NewGuid(),
                Valor = 1111
            }
        };

        await _producer.ProduceCommandAsync(command, cancellationToken);
        return Ok("Command published successfully!");
    }
}
```

<br>

## 🧠 Key Points

- Both **CommandProducer** and **EventProducer** share the same implementation logic.
- The difference lies only in **architectural intent** — commands indicate actions to be performed, events indicate actions that already happened.
- You can use dependency injection to directly obtain any producer type.