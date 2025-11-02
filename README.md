# RabbitMQ.FanoutExchange

This project demonstrates how to use **RabbitMQ Fanout Exchange** with multiple .NET 8 Web API applications.

## Project Overview

RabbitMQ.FanoutExchange/
├─ PublisherApi/ → Publishes messages to the fanout exchange
├─ SubscriberAApi/ → Listens on its own queue and receives broadcasts
├─ SubscriberBApi/ → Same as A, receives the same broadcast
├─ Shared/ → Common message DTO and RabbitMQ settings
└─ RabbitMQ.FanoutExchange.sln


### Message Flow

- **PublisherApi** publishes to the **`orders.fanout`** exchange.  
- Each subscriber defines its own queue (`queue.subscriberA`, `queue.subscriberB`) and binds it to this exchange.  
- The exchange broadcasts the message to **all** bound queues — both A and B receive it simultaneously.

---

## Shared Configuration

**Shared/RabbitSettings.cs**
```csharp
public static class RabbitSettings
{
    public const string ExchangeName = "orders.fanout";
    public const string ExchangeType = "fanout";

    public const string AmqpUrl =
        "amqps://<your-cloudamqp-url>";
}


Shared/OrderCreatedMessage.cs 

public class OrderCreatedMessage
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}


PublisherApi (sending)

channel.ExchangeDeclare(
    exchange: RabbitSettings.ExchangeName,
    type: RabbitSettings.ExchangeType);

var json = JsonSerializer.Serialize(order);
var body = Encoding.UTF8.GetBytes(json);

channel.BasicPublish(
    exchange: RabbitSettings.ExchangeName,
    routingKey: "",
    basicProperties: null,
    body: body);

    Subscribers (receiving)

    channel.ExchangeDeclare(RabbitSettings.ExchangeName, RabbitSettings.ExchangeType);

    var queueName = "queue.subscriberA"; // or queue.subscriberB
    channel.QueueDeclare(queueName, false, false, false, null);
    channel.QueueBind(queueName, RabbitSettings.ExchangeName, "");

    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (sender, args) =>
    {
        var json = Encoding.UTF8.GetString(args.Body.ToArray());
        var message = JsonSerializer.Deserialize<OrderCreatedMessage>(json);
        Console.WriteLine($"📩 [SubscriberA] {message?.CustomerName} - {message?.TotalAmount}");
    };
    channel.BasicConsume(queueName, true, consumer);


How to Test

1.Start SubscriberAApi and SubscriberBApi first.
Each console should print:
SubscriberX listening for messages...

2.Start PublisherApi and send a POST request to /api/orders:
{
  "orderId": "b2e10fa5-4e8a-41cc-b0f3-cc7b8978e700",
  "customerName": "Ahmet Yaman",
  "totalAmount": 350.00,
  "createdAtUtc": "2025-11-02T22:00:00Z"
}

3.Both subscriber consoles will show:
{
  "orderId": "b2e10fa5-4e8a-41cc-b0f3-cc7b8978e700",
  "customerName": "Ahmet Yaman",
  "totalAmount": 350.00,
  "createdAtUtc": "2025-11-02T22:00:00Z"
}

4.CloudAMQP Checkpoints

Exchange: orders.fanout

Queues: queue.subscriberA, queue.subscriberB

Bindings: each queue bound to the same exchange

Publish → Deliver → Ack: all counters increment simultaneously

Requirements

.NET 8 SDK

RabbitMQ account (CloudAMQP or local instance)


GitHub

https://github.com/ahmetyaman-ai/RabbitMQ.FanoutExchange