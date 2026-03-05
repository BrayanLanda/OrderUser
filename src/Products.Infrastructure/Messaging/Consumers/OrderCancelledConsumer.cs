using MassTransit;
using Microsoft.Extensions.Logging;
using Products.Domain.Entities.enums;
using Products.Domain.Repositories;
using Shared.Contracts.Events.Orders;

namespace Products.Infrastructure.Messaging.Consumers
{
    public class OrderCancelledConsumer(
     IProductRepository productRepo,
     IStockReservationRepository reservationRepo,
     ILogger<OrderCancelledConsumer> logger) : IConsumer<OrderCancelled>
    {
        public async Task Consume(ConsumeContext<OrderCancelled> context)
        {
            var msg = context.Message;
            logger.LogInformation("Liberando stock para orden cancelada {OrderId}", msg.OrderId);

            var reservations = await reservationRepo.GetByOrderIdAsync(msg.OrderId, context.CancellationToken);

            foreach (var reservation in reservations.Where(r => r.Status == ReservationStatus.Active))
            {
                var product = await productRepo.GetByIdAsync(reservation.ProductId, context.CancellationToken);
                if (product is null) continue;

                product.ReleaseStock(reservation.Quantity);
                await productRepo.UpdateAsync(product, context.CancellationToken);

                reservation.Release();
            }

            await reservationRepo.UpdateRangeAsync(reservations, context.CancellationToken);

            logger.LogInformation("Stock liberado correctamente para orden {OrderId}", msg.OrderId);
        }
    }
}