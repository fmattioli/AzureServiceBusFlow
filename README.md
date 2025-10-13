<h1 align="center">
  <img src="src\AzureServiceBusFlow\package-img.png" alt="AzureServiceBusFlow Logo" width="128">
  <br>
  AzureServiceBusFlow
</h1>

<h4 align="center">A lightweight, fluent, and extensible library for integrating .NET applications with Azure Service Bus.</h4>

<p align="center">
  <a href="https://github.com/your-org/AzureServiceBusFlow/actions/workflows/release.yml">
  </a>
  <a href="https://www.nuget.org/packages/AzureServiceBusFlow">
    <img src="https://img.shields.io/nuget/vpre/AzureServiceBusFlow.svg" alt="NuGet Version" />
  </a>
</p>

<p align="center">
  For advanced usage, configuration examples, and architecture details, please visit the <strong><a href="https://fmattioli.github.io/AzureServiceBusFlow/">Documentation</a></strong>.
</p>


## 🚀 Overview

**AzureServiceBusFlow (AsbFlow)** is a fluent integration library built to simplify working with **Azure Service Bus** in .NET applications.  
It provides a clean and expressive configuration model for producers, consumers, topics, and queues — inspired by the design philosophy of [KafkaFlow](https://github.com/Farfetch/kafkaflow).

With AsbFlow, you can easily register message producers and handlers, automatically ensure infrastructure existence, and handle message publishing or consumption through intuitive abstractions.


## ✨ Key Features

- **Fluent Configuration API** — Register and configure producers and consumers with a single builder chain.
- **Automatic Entity Creation** — Automatically ensures topics, queues, and subscriptions exist.
- **Producer Abstractions** — Send commands and events using `ICommandProducer` or `IEventProducer`.
- **Consumer Handlers** — Consume and process messages with your custom `IMessageHandler` implementations.
- **Dependency Injection Ready** — Seamless integration with Microsoft.Extensions.DependencyInjection.
- **Built-in Logging** — Integrated support for `ILogger` for structured observability.


## 🧰 Installation

Install from NuGet:

```bash
dotnet add package AzureServiceBusFlow
```

## ⚡ Quick Example
Below is a minimal example showing how to set up a producer and a consumer.

### 1. Define a Message

Messages are simple records or classes that implement **`IServiceBusMessage`**:
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

### 2. Create a Handler

Handlers implement the **`IMessageHandler<T>`** interface and define how messages are processed:
```csharp
public class CommandExample1Handler : IMessageHandler<ExampleCommand1>
{
    public Task HandleAsync(ExampleCommand1 message, ServiceBusReceivedMessage rawMessage, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
```

### 3. Register the Configuration

In your **`Program.cs`**:
```csharp
var azureServiceBusConfig = new AzureServiceBusConfiguration
{
    ConnectionString = "",
    ServiceBusReceiveMode = Azure.Messaging.ServiceBus.ServiceBusReceiveMode.ReceiveAndDelete,
    MaxConcurrentCalls = 10,
    MaxAutoLockRenewalDurationInSeconds = 1800,
    MaxRetryAttempts = 2
};

builder.Services.AddAzureServiceBus(cfg => cfg
    .ConfigureAzureServiceBus(azureServiceBusConfig)
    .AddProducer<ExampleCommand1>(p => p
        .EnsureQueueExists("command-queue-one")
        .WithCommandProducer()
        .ToQueue("command-queue-one"))
    .AddConsumer(c => c
        .FromQueue("command-queue-one")
        .AddHandler<ExampleCommand1, CommandExemple1Handler>())
    );
```

### 4. Send a Message
Inject an instance of **`ICommandProducer<T>`** or **`IEventProducer<T>`**, with the message type that is going to be sent, and call the corresponding method:
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
        return Ok();
    }
}
```
<br>

This example registers a **producer** that publishes to a queue and a **consumer** that listens to it.
AsbFlow ensures the queue exists automatically before usage.

## Learn More
For deeper insights into AzureServiceBusFlow — including infrastructure details, architecture, and extension points — visit the official [documentation](https://fmattioli.github.io/AzureServiceBusFlow)

Key pages include:
- [**Fundamental Concepts**](https://fmattioli.github.io/AzureServiceBusFlow/docs/messageConcept.html): Understand the difference between Commands, Events, and how message flow works.
- [**Key Components**](https://fmattioli.github.io/AzureServiceBusFlow/docs/messageComponent.html): Explore the key interfaces and how they structure the library.
- [**API Integration Guide**](https://fmattioli.github.io/AzureServiceBusFlow/docs/settingUp.html): Step-by-step instructions for integrating AsbFlow into your ASP.NET Core project.

> Tip: Each section of the documentation includes examples and diagrams that illustrate how the components interact within Azure Service Bus.
