//using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SyntheticArbitrage.Infrastructure.Services.Http;
using SyntheticArbitrage.Shared.ApiModels.Price;
using SyntheticArbitrage.Shared.RabbitMQModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SyntheticArbitrage.Infrastructure.Services.RabbitMQ;

public class RabbitMQConsumerService
{
    private readonly IConnectionFactory _factory;
    //private readonly IModel _channel;
    private readonly IBinanceHttpService _binanceHttpService;

    public RabbitMQConsumerService(IConnectionFactory factory, IBinanceHttpService binanceHttpService)
    {
        _factory = factory;
        _binanceHttpService = binanceHttpService;
    }

    public async void StartListeningForRequests()
    {
        IConnection conn = await _factory.CreateConnectionAsync();
        IChannel channel = await conn.CreateChannelAsync();
        await channel.QueueDeclareAsync("ticker_price_request", durable: true, exclusive: false, autoDelete: false);
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var correlationId = ea.BasicProperties?.CorrelationId;
            var responseQueue = ea.BasicProperties?.ReplyTo;
            var body = ea.Body.ToArray();
            var request = JsonSerializer.Deserialize<TickerPriceRequest>(body);

            // Обрабатываем запрос
            List<BinanceTickerPriceResponse> tickerPriceResponse = await _binanceHttpService.GetTickerPrices(request.Pair, request.IsQuarter);

            // Формируем ответ
            var responseMessage = JsonSerializer.Serialize(tickerPriceResponse);
            var responseBody = Encoding.UTF8.GetBytes(responseMessage);

            // Отправляем ответ обратно в очередь
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

            await channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: responseQueue,
                    mandatory: true,
                    basicProperties: properties,
                    body: responseBody
            );
        };

        await channel.BasicConsumeAsync(queue: "ticker_price_request", autoAck: true, consumer: consumer);
    }
}
