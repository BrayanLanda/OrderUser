using MassTransit;
using Microsoft.Extensions.Logging;
using Products.Domain.Entities;
using Products.Domain.Repositories;
using Shared.Contracts.Events.Orders;
using Shared.Contracts.Events.Products;

namespace Products.Infrastructure.Messaging.Consumers
{
    public class OrderCreatedConsumer(
    IProductRepository productRepo,
    IStockReservationRepository reservationRepo,
    IProductEventPublisher eventPublisher,
    ILogger<OrderCreatedConsumer> logger) : IConsumer<OrderCreated>
    {
        public async Task Consume(ConsumeContext<OrderCreated> context)
        {
            var msg = context.Message;
            logger.LogInformation("Verificando stock para orden {OrderId}", msg.OrderId);

            var productIds = msg.Items.Select(i => i.ProductId).ToList();
            var products = await productRepo.GetByIdsAsync(productIds, context.CancellationToken);

            var insufficientItems = new List<InsufficientItem>();

            foreach (var item in msg.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);

                if (product is null || !product.HasStock(item.Quantity))
                {
                    insufficientItems.Add(new InsufficientItem
                    {
                        ProductId = item.ProductId,
                        ProductName = product?.Name ?? "Producto no encontrado",
                        Requested = item.Quantity,
                        Available = product?.Stock ?? 0,
                    });
                }
            }

            if (insufficientItems.Count > 0)
            {
                logger.LogWarning("Stock insuficiente para orden {OrderId}: {Items}", msg.OrderId, insufficientItems.Count);

                await eventPublisher.PublishStockInsufficientAsync(new StockInsufficient
                {
                    CorrelationId = msg.CorrelationId,
                    OrderId = msg.OrderId,
                    Items = insufficientItems,
                }, context.CancellationToken);

                return;
            }

            var reservedItems = new List<ReservedItem>();

            foreach (var item in msg.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);
                product.ReserveStock(item.Quantity);
                await productRepo.UpdateAsync(product, context.CancellationToken);

                var reservation = StockReservation.Create(msg.OrderId, item.ProductId, item.Quantity);
                await reservationRepo.CreateAsync(reservation, context.CancellationToken);

                reservedItems.Add(new ReservedItem
                {
                    ProductId = item.ProductId,
                    QuantityReserved = item.Quantity,
                });
            }

            logger.LogInformation("Stock reservado correctamente para orden {OrderId}", msg.OrderId);

            await eventPublisher.PublishStockReservedAsync(new StockReserved
            {
                CorrelationId = msg.CorrelationId,
                OrderId = msg.OrderId,
                Items = reservedItems,
            }, context.CancellationToken);
        }
    }
}