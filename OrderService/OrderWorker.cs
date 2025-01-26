using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Events;
using OrderService.Models;
using Shared.MessageBus;
using Shared.Outbox;

namespace OrderService;

public class OrderWorker : BackgroundService
    {
        private readonly IMessageBus _messageBus;
        private readonly IServiceProvider _provider;
        private readonly ILogger<OrderWorker> _logger;

        public OrderWorker(IMessageBus messageBus, IServiceProvider provider, ILogger<OrderWorker> logger)
        {
            _messageBus = messageBus;
            _provider = provider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _provider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();

                try
                {
                    // Получаем необработанные сообщения из Outbox
                    var message = await dbContext.OutboxMessages
                        ?.Where(m => m.Processed == false) // Обрабатываем сообщения
                        .FirstOrDefaultAsync(stoppingToken);
                    
                    if (message == null)
                    {
                        _logger.LogInformation("No messages to process. Waiting for new messages...");
                        await Task.Delay(5000, stoppingToken); // Задержка перед следующей итерацией
                        continue;
                    }

                    try
                    {
                        // Десериализуем данные сообщения
                        var eventData = JsonSerializer.Deserialize<OrderCreatedEvent>(message.EventData);
                        if (eventData == null)
                        {
                            _logger.LogError("Failed to deserialize event data for message: {MessageId}", message.Id);
                            continue;
                        }

                        // Публикуем событие в RabbitMQ
                        _messageBus.Publish("order-events", eventData, "order");
                        // Публикация сообщения для inventory-service
                        _messageBus.Publish("order-events", eventData, "inventory");

                        // Публикация сообщения для payment-service
                        _messageBus.Publish("order-events", eventData, "payment");

                        // Помечаем сообщение как обработанное
                        message.Processed = true;
                        await dbContext.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation("Message {MessageId} processed successfully.", message.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message {MessageId}.", message.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox messages.");
                }

                // Задержка перед следующей итерацией
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
