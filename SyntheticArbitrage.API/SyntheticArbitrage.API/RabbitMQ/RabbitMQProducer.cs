using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using System.Text;

namespace SyntheticArbitrage.API.RabbitMQ;

public class RabbitMQProducer : IRabbitMQProducer
{
    private readonly ConnectionFactory _factory;
    //https://www.rabbitmq.com/client-libraries/dotnet-api-guide

    public RabbitMQProducer(IConfiguration configuration)
    {
        _factory = new ConnectionFactory()
        {
            HostName = configuration["localhost"],
            UserName = configuration["guest"],
            Password = configuration["guest"]
        };
    }

    public void SendMessage<T>(T message, string queueName)
    {
        using var connection = _factory.CreateConnectionAsync();
        //using var channel = connection.CreateChannelAsync();
        //channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

        //var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
        //channel.BasicPublish("", queueName, null, body);
    }
}
