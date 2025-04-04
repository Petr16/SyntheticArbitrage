namespace SyntheticArbitrage.API.RabbitMQ;

public interface IRabbitMQProducer
{
    Task SendMessageAsync<T>(T message, string queueName);
    Task<TResponse?> SendMessageAndGetResponseAsync<TRequest, TResponse>(TRequest message, string requestQueue);
}
