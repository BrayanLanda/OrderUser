using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Orders.Infrastructure.Saga;

public class OrderSagaStateMap : SagaClassMap<OrderSagaState>
{
    protected override void Configure(EntityTypeBuilder<OrderSagaState> entity, ModelBuilder model)
    {
        entity.ToTable("OrderSagaStates");
        entity.Property(x => x.CurrentState).HasMaxLength(64);
        entity.Property(x => x.UserEmail).HasMaxLength(200);
        entity.Property(x => x.TotalAmount).HasPrecision(18, 2);

        entity.Property(x => x.Items)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<SagaOrderItem>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new()
            );
    }
}
