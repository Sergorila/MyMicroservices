using System.Text.Json.Serialization;

namespace OrderService.Events;

public record OrderCreatedEvent(
    [property: JsonPropertyName("OrderId")] 
    Guid OrderId,
    
    [property: JsonPropertyName("ItemId")] 
    Guid ItemId,
    
    [property: JsonPropertyName("Amount")] 
    int Amount
);