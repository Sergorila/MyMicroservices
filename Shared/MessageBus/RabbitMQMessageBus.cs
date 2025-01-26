using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Shared.MessageBus;

public class RabbitMQMessageBus : IMessageBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    
    public RabbitMQMessageBus(string hostname)
    {
        var factory = new ConnectionFactory { HostName = hostname };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void Publish(string exchange, object message, string routingKey = "")
    {
        // Объявляем обменник с типом `direct`
        _channel.ExchangeDeclare(exchange, ExchangeType.Direct, durable: true);

        // Сериализуем сообщение и публикуем его с указанным routing key
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        _channel.BasicPublish(exchange, routingKey, body: body);
    }

    public void Subscribe<TEvent>(string exchange, string queue, Func<TEvent, Task> handler, string routingKey = "")
    {
        // Объявляем обменник с типом `direct`
        _channel.ExchangeDeclare(exchange, ExchangeType.Direct, durable: true);

        // Объявляем очередь
        _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);

        // Привязываем очередь к обменнику с указанным routing key
        _channel.QueueBind(queue, exchange, routingKey);
        
        // Создаем потребителя
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = JsonSerializer.Deserialize<TEvent>(Encoding.UTF8.GetString(body));
            if (message != null)
            {
                await handler(message);
            }
            _channel.BasicAck(ea.DeliveryTag, false);
        };
        
        // Начинаем потребление сообщений из очереди
        _channel.BasicConsume(queue, false, consumer);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}