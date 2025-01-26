using System.Text.Json;
using InventoryService.Data;
using InventoryService.Events;
using Microsoft.EntityFrameworkCore;
using OrderService.Events;
using Shared.MessageBus;
using Shared.Outbox;

namespace InventoryService;

public class InventoryWorker : BackgroundService
{
    private readonly IMessageBus _messageBus;
    private readonly IServiceProvider _provider;
    private readonly ILogger<InventoryWorker> _logger;

    public InventoryWorker(IMessageBus messageBus, IServiceProvider provider, ILogger<InventoryWorker> logger)
    {
        _messageBus = messageBus;
        _provider = provider;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Подписываемся на событие OrderCreatedEvent
        _messageBus.Subscribe<OrderCreatedEvent>("order-events", "inventory-service", async @event =>
        {
            _logger.LogInformation("InventoryWorker received OrderCreatedEvent: OrderId={OrderId}, ItemId={ItemId}, Amount={Amount}", 
                @event.OrderId, @event.ItemId, @event.Amount);
            using var scope = _provider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = nameof(InventoryUpdatedEvent),
                EventData = JsonSerializer.Serialize(new InventoryUpdatedEvent(
                    @event.OrderId,
                    @event.ItemId,
                    @event.Amount)),
                Processed = false
            };
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            dbContext.OutboxMessages.Add(outboxMessage);
            await dbContext.SaveChangesAsync();
            
            InventoryUpdatedEvent itemEvent = JsonSerializer.Deserialize<InventoryUpdatedEvent>(outboxMessage.EventData);
            
            // Имитация обновления инвентаря
            await Task.Delay(5000); // Задержка для имитации
            
            outboxMessage.Processed = true;
            dbContext.Update(outboxMessage);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Inventory updated: ItemId={ItemId}", 
                itemEvent.ItemId);
        }, "inventory");

        return Task.CompletedTask;
    }
}