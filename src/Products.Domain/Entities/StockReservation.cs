using Products.Domain.Entities.enums;

namespace Products.Domain.Entities;

public class StockReservation
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public ReservationStatus Status { get; private set; } = ReservationStatus.Active;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private StockReservation() { }

    public static StockReservation Create(Guid orderId, Guid productId, int quantity)
    {
        return new StockReservation
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
        };
    }

    public void Release()
    {
        Status = ReservationStatus.Released;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Confirm()
    {
        Status = ReservationStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }
}
