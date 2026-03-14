using MassTransit;
using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;
using Orders.Infrastructure.Saga;

namespace Orders.Infrastructure.Persistence;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderSagaState> OrderSagaStates => Set<OrderSagaState>();

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

        // ── Saga State ────────────────────────────────────────
        modelBuilder.Entity<OrderSagaState>(e =>
        {
            e.ToTable("OrderSagaStates");
            e.HasKey(x => x.CorrelationId);
            e.Property(x => x.CurrentState).HasMaxLength(64);
            e.Property(x => x.UserEmail).HasMaxLength(200);
            e.Property(x => x.TotalAmount).HasPrecision(18, 2);
            e.Property(x => x.Items)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<SagaOrderItem>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new()
                );
        });

        // ── Saga State Machine (MassTransit) ──────────────────
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
    }
}