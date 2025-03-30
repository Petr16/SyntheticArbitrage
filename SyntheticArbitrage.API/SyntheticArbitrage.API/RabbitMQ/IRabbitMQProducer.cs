namespace SyntheticArbitrage.API.RabbitMQ;

public interface IRabbitMQProducer
{
    void SendMessage<T>(T message, string queueName);
}
