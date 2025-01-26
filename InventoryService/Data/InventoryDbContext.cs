using Microsoft.EntityFrameworkCore;
using Shared.Outbox;

namespace InventoryService.Data;

public class InventoryDbContext : DbContext
{
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql("Host=localhost;Database=MyMicroservices;Username=postgres;Password=0000");
    }
}