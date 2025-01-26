using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using Shared.MessageBus;
using Shared.Outbox;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OutboxDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("myDb")));

builder.Services.AddSingleton<IMessageBus>(new RabbitMQMessageBus("localhost"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Web API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web API v1"));
}

app.MapControllers();
app.Run();