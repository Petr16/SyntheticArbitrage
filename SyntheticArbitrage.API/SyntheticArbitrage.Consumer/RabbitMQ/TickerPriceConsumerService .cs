using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SyntheticArbitrage.Infrastructure.Services.Http;
using SyntheticArbitrage.Shared.RabbitMQModels;
using System.Text;
using System.Text.Json;

namespace SyntheticArbitrage.Consumer.RabbitMQ;

public class TickerPriceConsumerService : BackgroundService
{
    //private IConnection _connection;
    //private IChannel _channel;
    private IBinanceHttpService _binanceHttpService;
    private readonly ConnectionFactory _factory;

    public TickerPriceConsumerService(
        IConfiguration configuration,
        IBinanceHttpService binanceHttpService)
    {
        _binanceHttpService = binanceHttpService;
        // Не забудьте вынести значения "localhost" и "MyQueue"
        // в файл конфигурации
        _factory = new ConnectionFactory()
        {
            HostName = configuration["RabbitMQ:HostName"],
            UserName = configuration["RabbitMQ:UserName"],
            Password = configuration["RabbitMQ:Password"],
            //ConsumerDispatchConcurrency
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await _factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync("ticker_price_request", durable: true, exclusive: false, autoDelete: false);
        await channel.QueueDeclareAsync("ticker_price_response", durable: true, exclusive: false, autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var request = JsonSerializer.Deserialize<TickerPriceRequest>(body);

            if (request is null) return;

            var result = await _binanceHttpService.GetTickerPrices(request.Pair, request.IsQuarter);

            var responseBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result));

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = (DeliveryModes)2, // 2 = Persistent
                Headers = new Dictionary<string, object>
                {
                    { "created_at", DateTime.UtcNow.ToString("o") },
                    { "source", "SyntheticArbitrage.API" }
                }
            };

            await channel.BasicPublishAsync(exchange: "",
                                            routingKey: "ticker_price_response",
                                            mandatory: true,
                                            basicProperties: properties,
                                            body: responseBody);
        };

        await channel.BasicConsumeAsync(queue: "ticker_price_request",
                                        autoAck: true,
                                        consumer: consumer);

        Console.WriteLine("Consumer запущен. Ожидаем сообщения...");
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
