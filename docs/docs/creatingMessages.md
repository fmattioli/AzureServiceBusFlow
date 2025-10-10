## Messages

The **AzureServiceBusFlow** defines a standard approach for creating messages that will be published to the bus. This ensures that the **MessageHandler** receives the correct message type and processes it properly.  

---

### ✉️ Creating Messages

To create these messages, you need to define a record or class that implements **`IServiceBusMessage`** interface from the **AzureServiceBusFlow** package.

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

The **RoutingKey** and **CreatedDate** properties are used internally by **AzureServiceBusFlow**, while **ExampleMessage** represents the actual content of the message being sent.

The message above is just an example and the **Content** can be of any type, such as a class, record, struct, integer, string, IEnumerable...

---

To register which messages will be sent to the bus, we need to configure the **Producers** to publish the messages. ``Check out the next page to know how to register it.``