using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OrderService.Data;
using OrderService.Events;
using OrderService.Models;
using Shared.MessageBus;
using Shared.Outbox;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly OutboxDbContext _context;
    private readonly IMessageBus _messageBus;

    private readonly List<Guid> items = new List<Guid>()
        { Guid.Parse("550e8400-e29b-41d4-a716-446655440001"), Guid.Parse("550e8400-e29b-41d4-a716-446655440000") };

    public OrderController(OutboxDbContext context, IMessageBus messageBus)
    {
        _context = context;
        _messageBus = messageBus;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder()
    {
        try
        {
            var itemId = new Random().Next(0, items.Count);
            var order = new Order
            {
                Id = Guid.NewGuid(),
                ItemId = items[itemId],
                Amount = 2,
                Status = OrderStatus.Created,
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();

            // Явная проверка GUID
            if (order.Id == Guid.Empty)
            {
                throw new InvalidOperationException("Generated OrderId is empty");
            }

            var orderEvent = new OrderCreatedEvent(
                order.Id,
                order.ItemId,
                order.Amount);

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = nameof(OrderCreatedEvent),
                EventData = JsonSerializer.Serialize(orderEvent, new JsonSerializerOptions 
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }),
                Processed = false
            };
            
            _context.OutboxMessages.Add(outboxMessage);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Created($"/orders/{order.Id}", order);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}