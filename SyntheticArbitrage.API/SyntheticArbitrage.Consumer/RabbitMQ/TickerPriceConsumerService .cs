using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SyntheticArbitrage.Infrastructure.Services.Http;
using SyntheticArbitrage.Shared.RabbitMQModels;
using System.Text;
using System.Text.Json;

namespace SyntheticArbitrage.Consumer.RabbitMQ;

public class TickerPriceConsumerService : BackgroundService
{
    private IBinanceHttpService _binanceHttpService;
    private readonly ConnectionFactory _factory;
    private readonly RabbitMQConfig _config;

    public TickerPriceConsumerService(
        IOptions<RabbitMQConfig> configOptions,
        IBinanceHttpService binanceHttpService)
    {
        _config = configOptions.Value;
        _binanceHttpService = binanceHttpService;
        _factory = new ConnectionFactory()
        {
            HostName = _config.HostName,
            UserName = _config.UserName,
            Password = _config.Password,
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //Во многих старых примерах был using var
        //Но тогда, если метод завершится до получения ответа,
        //using вызовет Dispose() → закроется канал → tcs.TrySetResult() не сработает
        //(потому что потребитель отключён до того, как пришёл ответ).
        IConnection connection = await _factory.CreateConnectionAsync();
        IChannel channel = await connection.CreateChannelAsync();

        //входящая очередь, в неё Producer будет отправлять запросы
        await channel.QueueDeclareAsync("ticker_price_request", durable: true, exclusive: false, autoDelete: false);
        //ответная очередь, куда Consumer будет отправлять ответы
        await channel.QueueDeclareAsync("ticker_price_response", durable: true, exclusive: false, autoDelete: false);

        //асинхронный слушатель сообщений, который будет обрабатывать входящие сообщения.
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var request = JsonSerializer.Deserialize<TickerPriceRequest>(body);

            if (request is null) return;

            Console.WriteLine("Start Recieve consumer");
            var jsonString = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(jsonString);
            //получаем цены квартальной и би-квартальной пары
            var result = await _binanceHttpService.GetTickerPrices(request.Pair, request.IsQuarter);

            var responseBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result));

            var jsonStringResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine("responseBody: "+ jsonStringResult);

            var replyProps = new BasicProperties
            {
                ContentType = "application/json",
                CorrelationId = ea.BasicProperties?.CorrelationId //чтобы ответ пошел туда, где ждет Producer
            };

            await channel.BasicPublishAsync(exchange: "",
                                            routingKey: ea.BasicProperties?.ReplyTo, // сюда отправим ответ "ticker_price_response",
                                            mandatory: true,
                                            basicProperties: replyProps,
                                            body: responseBody);
        };

        await channel.BasicConsumeAsync(queue: "ticker_price_request",
                                        autoAck: true,//сообщение считается "обработанным" сразу при получении
                                        consumer: consumer);

        Console.WriteLine("Consumer запущен. Ожидаем сообщения...");
        //чтобы сервис не завершался и продолжал слушать очередь
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
