using Orders.Domain.Entities.Enums;

namespace Orders.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string UserEmail { get; private set; } = default!;
    public OrderStatus Status { get; private set; } = OrderStatus.AwaitingUserValidation;
    public decimal TotalAmount { get; private set; }
    public List<OrderItem> Items { get; private set; } = [];
    public string? CancellationReason { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private Order() { }

    public static Order Create(Guid userId, string userEmail, List<OrderItem> items)
    {
        if (!items.Any())
            throw new ArgumentException("La orden debe tener al menos un item.");

        var order = new Order
        {
            UserId = userId,
            UserEmail = userEmail,
            Items = items,
            TotalAmount = items.Sum(i => i.UnitPrice * i.Quantity),
        };

        return order;
    }

    public void MarkAsPending()
    {
        Status = OrderStatus.Pending;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Confirm()
    {
        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        Status = OrderStatus.Cancelled;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }
}
