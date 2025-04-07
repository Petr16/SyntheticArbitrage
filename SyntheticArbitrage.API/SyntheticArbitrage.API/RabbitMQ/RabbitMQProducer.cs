using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;
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
    private readonly RabbitMQConfig _config;
    //https://www.rabbitmq.com/client-libraries/dotnet-api-guide

    public RabbitMQProducer(/*IConfiguration configuration*/IOptions<RabbitMQConfig> configOptions)
    {
        //_factory = new ConnectionFactory()
        //{
        //    HostName = configuration["RabbitMQ:HostName"],
        //    UserName = configuration["RabbitMQ:UserName"],
        //    Password = configuration["RabbitMQ:Password"]
        //};

        _config = configOptions.Value;
        _factory = new ConnectionFactory()
        {
            HostName = _config.HostName,
            UserName = _config.UserName,
            Password = _config.Password,
        };
    }

    //Если достаточно просто отправить в одну сторону
    //public async Task SendMessageAsync<T>(T message, string queueName)
    //{
    //    IConnection conn = await _factory.CreateConnectionAsync();
    //    IChannel channel = await conn.CreateChannelAsync();
    //    channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);

    //    var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
    //    var properties = new BasicProperties
    //    {
    //        ContentType = "application/json",
    //        DeliveryMode = (DeliveryModes)2, // 2 = Persistent
    //        Headers = new Dictionary<string, object>
    //        {
    //            { "created_at", DateTime.UtcNow.ToString("o") },
    //            { "source", "SyntheticArbitrage.API" }
    //        }
    //    };
    //    await channel.BasicPublishAsync(exchange: "",
    //                                    routingKey: queueName,
    //                                    mandatory: true,
    //                                    basicProperties: properties,
    //                                    body: messageBody);
    //}

    /// <summary>
    /// Отправить запрос, выполнить его консьюмером и принять от него сериализованный ответ
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="message"></param>
    /// <param name="requestQueue"></param>
    /// <returns></returns>
    public async Task<TResponse?> SendMessageAndGetResponseAsync<TRequest, TResponse>(TRequest message, string requestQueue)
    {
        var connection = await _factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        // Создание временной очереди для ответа
        var replyQueue = await channel.QueueDeclareAsync(queue: "", exclusive: true);
        var consumer = new AsyncEventingBasicConsumer(channel);

        var tcs = new TaskCompletionSource<TResponse>();
        var correlationId = Guid.NewGuid().ToString();

        // Обработчик, который будет вызван, когда придет ответ
        consumer.ReceivedAsync += async (model, ea) =>
        {
            Console.WriteLine($"ea.BasicProperties.CorrelationId: {ea.BasicProperties.CorrelationId} == correlationId: {correlationId}");
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                var responseJson = Encoding.UTF8.GetString(ea.Body.ToArray());
                var response = JsonSerializer.Deserialize<TResponse>(responseJson);
                
                tcs.TrySetResult(response!);// сеттим результат
            }
            // подтверждение вручную, если autoAck: false у BasicConsumeAsync
            //await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
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
            };

        // Отправляем сообщение в очередь запроса
        var messageBodyBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: requestQueue,
            mandatory: true,
            basicProperties: props,
            body: messageBodyBytes);

        //дожидаемся ответа и закрываем соединение, иначе будет жрать память
        TResponse? result = await tcs.Task;

        await channel.CloseAsync();      // Закрываем канал
        await connection.CloseAsync();   // Закрываем соединение

        return result;
    }

}
