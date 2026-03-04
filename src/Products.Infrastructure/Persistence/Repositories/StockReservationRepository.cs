using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;
using Products.Domain.Repositories;

namespace Products.Infrastructure.Persistence.Repositories
{
    public class StockReservationRepository(ProductsDbContext context) : IStockReservationRepository
    {
        public async Task<List<StockReservation>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default) =>
        await context.StockReservations.Where(r => r.OrderId == orderId).ToListAsync(ct);

        public async Task CreateAsync(StockReservation reservation, CancellationToken ct = default)
        {
            context.StockReservations.Add(reservation);
            await context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(StockReservation reservation, CancellationToken ct = default)
        {
            context.StockReservations.Update(reservation);
            await context.SaveChangesAsync(ct);
        }

        public async Task UpdateRangeAsync(List<StockReservation> reservations, CancellationToken ct = default)
        {
            context.StockReservations.UpdateRange(reservations);
            await context.SaveChangesAsync(ct);
        }
    }
}