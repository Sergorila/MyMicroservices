using Microsoft.EntityFrameworkCore;
using OrderService;
using OrderService.Data;
using Shared.MessageBus;
using Shared.Outbox;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<OutboxDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("myDb")));

builder.Services.AddSingleton<IMessageBus>(new RabbitMQMessageBus("localhost"));
builder.Services.AddHostedService<OrderWorker>();

var host = builder.Build();
host.Run();