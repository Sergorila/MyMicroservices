namespace Shared.MessageBus;

public interface IMessageBus
{
    void Publish(string exchange, object message, string routingKey);
    void Subscribe<TEvent>(string exchange, string queue, Func<TEvent, Task> handler, string routingKey);
}