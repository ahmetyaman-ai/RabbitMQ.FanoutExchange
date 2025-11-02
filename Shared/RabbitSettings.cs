namespace Shared;

public static class RabbitSettings
{
    // Exchange ve Queue bilgileri
    public const string ExchangeName = "orders.fanout";  // fanout tipi exchange
    public const string ExchangeType = "fanout";

    // CloudAMQP bağlantı adresin (kendi URL’ni koy)
    public const string AmqpUrl =
        "amqps://mzqboxum:XoLgxnB0cxyK-ZtQ4tnfxrbHSxMkuWF3@moose.rmq.cloudamqp.com/mzqboxum";
}
