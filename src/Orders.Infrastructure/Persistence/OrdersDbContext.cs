using MassTransit;
using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Persistence;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.UserEmail).IsRequired().HasMaxLength(200);
            e.Property(o => o.TotalAmount).HasPrecision(18, 2);
            e.Property(o => o.Status).HasConversion<string>();
            e.Property(o => o.CancellationReason).HasMaxLength(500);
            e.HasMany(o => o.Items).WithOne().HasForeignKey(i => i.OrderId);
            e.HasIndex(o => o.UserId);
        });

        // ── OrderItem ─────────────────────────────────────────
        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
            e.Property(i => i.UnitPrice).HasPrecision(18, 2);
        });

        // ── Saga State Machine (MassTransit) ──────────────────
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
    }
}