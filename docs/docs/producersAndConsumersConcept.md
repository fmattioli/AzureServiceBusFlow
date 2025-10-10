# Producers and Consumers

In **AzureServiceBusFlow**, communication between services happens through **producers** and **consumers**, which abstract away the complexity of connecting to Azure Service Bus.

<br>

## 🚀 Producers

A **producer** (also known as a **sender**) is responsible for **sending messages** to a queue or topic.

With **AsbFlow**, producers are registered and configured fluently using simple definitions.  
They can send either **commands** or **events**, depending on the communication model.

Example responsibilities:
- Serializing the message payload
- Sending the message to the appropriate Azure Service Bus entity
- Logging and error handling

<br>

## 📥 Consumers

A **consumer** (or **receiver**) is responsible for **receiving and processing messages** from queues or subscriptions.

In **AsbFlow**, consumers are also registered fluently and can automatically bind to queues or subscriptions, depending on message type.

Example responsibilities:
- Deserializing the message
- Executing the related business logic
- Handling retries and exceptions

<br>

## 🧩 Relationship

| Component | Description |
| :---------- | :------------ |
| **Producer** | Sends messages to Azure Service Bus (queues or topics). |
| **Consumer** | Listens for and processes incoming messages. |
| **Queue/Topic** | The channel through which messages flow. |

This abstraction makes it easy to maintain a **clean architecture**, where producers and consumers are clearly separated and follow consistent configuration patterns.
