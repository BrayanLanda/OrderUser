using Shared.Contracts.Events.Products;

namespace Products.Infrastructure.Messaging;
public interface IProductEventPublisher
{
    Task PublishStockReservedAsync(StockReserved message, CancellationToken ct = default);
    Task PublishStockInsufficientAsync(StockInsufficient message, CancellationToken ct = default);
}
