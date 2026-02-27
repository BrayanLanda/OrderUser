namespace Shared.Contracts.Events.Orders;

public class OrderCreated
{
    public Guid CorrelationId { get; init; }
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public List<OrderItem> Items { get; set; } = [];
    public decimal TotalAmount { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public record OrderConfirmed
{
    public Guid CorrelationId { get; init; }
    public Guid OrderId       { get; init; }
    public Guid UserId        { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public record OrderCancelled
{
    public Guid CorrelationId { get; init; }
    public Guid OrderId       { get; init; }
    public Guid UserId        { get; init; }
    public string Reason      { get; init; } = default!;
    public List<OrderItem> Items { get; init; } = [];  // Para que Products sepa qué liberar
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public record OrderItem
{
    public Guid ProductId      { get; init; }
    public string ProductName  { get; init; } = default!;
    public int Quantity        { get; init; }
    public decimal UnitPrice   { get; init; }
}