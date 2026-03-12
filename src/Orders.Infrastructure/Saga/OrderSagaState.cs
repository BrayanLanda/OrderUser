using MassTransit;

namespace Orders.Infrastructure.Saga;

public class OrderSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }  // = OrderId
    public string CurrentState { get; set; } = default!;
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public List<SagaOrderItem> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}