using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Shared.Contracts.Events.Products;

namespace Products.Infrastructure.Messaging;
public class ProductEventPublisher(
ITopicProducer<StockReserved> reservedProducer,
ITopicProducer<StockInsufficient> insufficientProducer) : IProductEventPublisher
{
    public async Task PublishStockReservedAsync(StockReserved message, CancellationToken ct = default)
        => await reservedProducer.Produce(message, ct);

    public async Task PublishStockInsufficientAsync(StockInsufficient message, CancellationToken ct = default)
        => await insufficientProducer.Produce(message, ct);
}
