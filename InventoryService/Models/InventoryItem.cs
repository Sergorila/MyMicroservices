namespace InventoryService.Data;

public class InventoryItem
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
}