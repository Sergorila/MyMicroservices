using System.Text.Json.Serialization;

namespace InventoryService.Events;

public record InventoryUpdatedEvent(
    [property: JsonPropertyName("OrderId")] 
    Guid OrderId,
    
    [property: JsonPropertyName("ItemId")] 
    Guid ItemId,
    
    [property: JsonPropertyName("Amount")] 
    int Amount
);