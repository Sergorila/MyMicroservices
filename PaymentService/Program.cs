using Microsoft.EntityFrameworkCore;
using PaymentService;
using PaymentService.Data;
using Shared.MessageBus;
using Shared.Outbox;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<OutboxDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("myDb")));

// Регистрация RabbitMQ и PaymentWorker
builder.Services.AddSingleton<IMessageBus>(new RabbitMQMessageBus("localhost"));
builder.Services.AddHostedService<PaymentWorker>();

var host = builder.Build();
host.Run();