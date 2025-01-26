using Microsoft.EntityFrameworkCore;
using OrderService.Models;
using Shared.Outbox;

namespace OrderService.Data;

public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql("Host=localhost;Database=MyMicroservices;Username=postgres;Password=0000");
    }
}
