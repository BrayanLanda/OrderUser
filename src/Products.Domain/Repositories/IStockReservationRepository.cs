using Products.Domain.Entities;

namespace Products.Domain.Repositories
{
    public interface IStockReservationRepository
    {
        Task<List<StockReservation>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
        Task CreateAsync(StockReservation reservation, CancellationToken ct = default);
        Task UpdateAsync(StockReservation reservation, CancellationToken ct = default);
        Task UpdateRangeAsync(List<StockReservation> reservations, CancellationToken ct = default);
    }
}