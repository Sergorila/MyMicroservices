using InventoryService;
using InventoryService.Data;
using Microsoft.EntityFrameworkCore;
using Shared.MessageBus;
using Shared.Outbox;

var builder = Host.CreateApplicationBuilder(args);

// Настройка базы данных
builder.Services.AddDbContext<OutboxDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("myDb")));

// Регистрация RabbitMQ и InventoryWorker
builder.Services.AddSingleton<IMessageBus>(new RabbitMQMessageBus("localhost"));
builder.Services.AddHostedService<InventoryWorker>();

var host = builder.Build();
host.Run();