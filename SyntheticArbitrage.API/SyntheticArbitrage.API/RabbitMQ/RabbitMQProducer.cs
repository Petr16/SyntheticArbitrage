using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SyntheticArbitrage.Shared.RabbitMQModels;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace SyntheticArbitrage.API.RabbitMQ;

public class RabbitMQProducer : IRabbitMQProducer
{
    private readonly ConnectionFactory _factory;
    //private readonly IConnection _connection;
    //private readonly IModel _channel;
    //private const string QueueName = "ticker_price_request";
    //https://www.rabbitmq.com/client-libraries/dotnet-api-guide

    public RabbitMQProducer(IConfiguration configuration)
    {
        _factory = new ConnectionFactory()
        {
            HostName = configuration["RabbitMQ:HostName"],
            UserName = configuration["RabbitMQ:UserName"],
            Password = configuration["RabbitMQ:Password"]
        };
        //_factory.Uri = new Uri("amqp://user:pass@hostName:port/vhost");
        //var factory = new ConnectionFactory() { HostName = "localhost" }; // или другой хост
        //_connection = await _factory.CreateConnectionAsync();
        //_channel = _connection.CreateModel();
        //_channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    public async Task SendMessageAsync<T>(T message, string queueName)
    {
        IConnection conn = await _factory.CreateConnectionAsync();
        IChannel channel = await conn.CreateChannelAsync();
        channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);

        var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
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
                                        routingKey: queueName,
                                        mandatory: true,
                                        basicProperties: properties,
                                        body: messageBody);
    }

    //public void SendTickerRequest(TickerPriceRequest request, string queueName)
    //{
    //    var message = JsonSerializer.Serialize(request);
    //    var body = Encoding.UTF8.GetBytes(message);

    //    _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
    //}

    public async Task<TResponse?> SendMessageAndGetResponseAsync<TRequest, TResponse>(TRequest message, string requestQueue)
    {
        using var connection = await _factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        // Создание временной очереди для ответа
        var replyQueue = await channel.QueueDeclareAsync(queue: "", exclusive: true);
        var consumer = new AsyncEventingBasicConsumer(channel);

        var tcs = new TaskCompletionSource<TResponse>();
        var correlationId = Guid.NewGuid().ToString();

        // Обработчик, который будет вызван, когда придет ответ
        consumer.ReceivedAsync += async (model, ea) =>
        {
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                var responseJson = Encoding.UTF8.GetString(ea.Body.ToArray());
                var response = JsonSerializer.Deserialize<TResponse>(responseJson);
                tcs.SetResult(response!);// Возвращаем результат
            }

            await Task.Yield();
        };

        // Подключаемся к очереди ответов
        await channel.BasicConsumeAsync(consumer: consumer, queue: replyQueue.QueueName, autoAck: true);

        // Устанавливаем свойства сообщения
        var props = new BasicProperties
            {
                CorrelationId = correlationId,
                ReplyTo = replyQueue.QueueName,
                ContentType = "application/json"
            };//channel.CreateBasicProperties();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueue.QueueName;
        props.ContentType = "application/json";

        // Отправляем сообщение в очередь запроса
        var messageBodyBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: requestQueue,
            mandatory: true,
            basicProperties: props,
            body: messageBodyBytes);

        // Ожидаем результат
        return await tcs.Task;
    }

}
