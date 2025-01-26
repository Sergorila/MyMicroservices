namespace OrderService.Models;

public class Order
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public int Amount { get; set; }
    public OrderStatus Status { get; set; }
}