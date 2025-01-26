namespace Shared.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; }
    public string EventData { get; set; }
    public bool Processed { get; set; }
}