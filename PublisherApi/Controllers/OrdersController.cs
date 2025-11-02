using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using Shared;
using System.Text;
using System.Text.Json;

namespace PublisherApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpPost]
    public IActionResult PublishOrder([FromBody] OrderCreatedMessage order)
    {
        var factory = new ConnectionFactory { Uri = new Uri(RabbitSettings.AmqpUrl) };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        // Fanout exchange oluşturuluyor
        channel.ExchangeDeclare(
            exchange: RabbitSettings.ExchangeName,
            type: RabbitSettings.ExchangeType,
            durable: false,
            autoDelete: false,
            arguments: null);

        // Mesaj oluşturuluyor
        var json = JsonSerializer.Serialize(order);
        var body = Encoding.UTF8.GetBytes(json);

        // Fanout tipinde routingKey önemsizdir, boş bırakılır
        channel.BasicPublish(
            exchange: RabbitSettings.ExchangeName,
            routingKey: "",
            basicProperties: null,
            body: body);

        return Ok("Order broadcasted via fanout exchange.");
    }
}
