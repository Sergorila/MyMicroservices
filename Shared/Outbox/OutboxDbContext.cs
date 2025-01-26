using Microsoft.EntityFrameworkCore;

namespace Shared.Outbox;

public class OutboxDbContext : DbContext
{
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    
    public OutboxDbContext(DbContextOptions<OutboxDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql("Host=localhost;Database=MyMicroservices;Username=postgres;Password=0000");
    }
}