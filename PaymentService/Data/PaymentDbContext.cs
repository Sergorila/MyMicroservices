using Microsoft.EntityFrameworkCore;
using Shared.Outbox;

namespace PaymentService.Data;

public class PaymentDbContext : DbContext
{
    public DbSet<Payment> Payments { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql("Host=localhost;Database=MyMicroservices;Username=postgres;Password=0000");
    }
}