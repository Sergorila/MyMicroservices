using System.Text.Json.Serialization;

namespace PaymentService.Events;

public record PaymentProcessedEvent(
    [property: JsonPropertyName("OrderId")] 
    Guid OrderId,
    
    [property: JsonPropertyName("ItemId")] 
    Guid ItemId,
    
    [property: JsonPropertyName("Amount")] 
    int Amount);