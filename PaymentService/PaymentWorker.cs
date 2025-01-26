using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderService.Events;
using PaymentService.Data;
using PaymentService.Events;
using Shared.MessageBus;
using Shared.Outbox;

namespace PaymentService;

public class PaymentWorker : BackgroundService
{
    private readonly IMessageBus _messageBus;
    private readonly IServiceProvider _provider;
    private readonly ILogger<PaymentWorker> _logger;

    public PaymentWorker(IMessageBus messageBus, IServiceProvider provider, ILogger<PaymentWorker> logger)
    {
        _messageBus = messageBus;
        _provider = provider;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Подписываемся на событие OrderCreatedEvent
        _messageBus.Subscribe<OrderCreatedEvent>("order-events", "payment-service", async @event =>
        {
            _logger.LogInformation("PaymentWorker received OrderCreatedEvent: OrderId={OrderId}, ItemId={ItemId}, Amount={Amount}", 
                @event.OrderId, @event.ItemId, @event.Amount);
            using var scope = _provider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();

            var payment = new Payment
            {
                OrderId = @event.OrderId,
                Amount = @event.Amount,
                Status = PaymentStatus.Completed
            };

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = nameof(PaymentProcessedEvent),
                EventData = JsonSerializer.Serialize(new PaymentProcessedEvent(
                    @event.OrderId,
                    @event.ItemId,
                    @event.Amount))
            };

            // Сохраняем платеж и событие в Outbox
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            dbContext.OutboxMessages.Add(outboxMessage);
            await dbContext.SaveChangesAsync();
            
            
            // Имитация обработки платежа
            await Task.Delay(10000); // Задержка для имитации
            
            outboxMessage.Processed = true;
            dbContext.Update(outboxMessage);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Payment processed: OrderId={OrderId}, Amount={Amount}", 
                @event.OrderId, @event.Amount);
        }, "payment");

        return Task.CompletedTask;
    }
}