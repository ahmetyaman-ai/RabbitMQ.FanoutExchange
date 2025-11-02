using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System.Text;
using System.Text.Json;

namespace SubscriberBApi.Services;

public class FanoutListener : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { Uri = new Uri(RabbitSettings.AmqpUrl) };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        // Exchange tanımı (Publisher ile aynı olmalı)
        channel.ExchangeDeclare(
            exchange: RabbitSettings.ExchangeName,
            type: RabbitSettings.ExchangeType,
            durable: false,
            autoDelete: false,
            arguments: null);

        // SubscriberB'ye özel kuyruk
        var queueName = "queue.subscriberB";
        channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        channel.QueueBind(queue: queueName, exchange: RabbitSettings.ExchangeName, routingKey: "");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (sender, args) =>
        {
            var json = Encoding.UTF8.GetString(args.Body.ToArray());
            var message = JsonSerializer.Deserialize<OrderCreatedMessage>(json);
            Console.WriteLine($"📩 [SubscriberB] Received: {message?.CustomerName} - {message?.TotalAmount}");
        };

        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

        Console.WriteLine("✅ SubscriberB listening for messages...");

        // Servis aktif kalsın
        while (!stoppingToken.IsCancellationRequested)
        {
            Thread.Sleep(1000);
        }

        return Task.CompletedTask;
    }
}
