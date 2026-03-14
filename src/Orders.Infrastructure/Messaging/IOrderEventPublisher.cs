using MassTransit;
using Shared.Contracts.Events.Orders;

namespace Orders.Infrastructure.Messaging;

public interface IOrderEventPublisher
{
    Task PublishOrderCreatedAsync(OrderCreated message, CancellationToken ct = default);
    Task PublishOrderConfirmedAsync(OrderConfirmed message, CancellationToken ct = default);
    Task PublishOrderCancelledAsync(OrderCancelled message, CancellationToken ct = default);
}

public class OrderEventPublisher(
    ITopicProducer<OrderCreated> orderCreatedProducer,
    ITopicProducer<OrderConfirmed> orderConfirmedProducer,
    ITopicProducer<OrderCancelled> orderCancelledProducer) : IOrderEventPublisher
{
    public async Task PublishOrderCreatedAsync(OrderCreated message, CancellationToken ct = default)
        => await orderCreatedProducer.Produce(message, ct);

    public async Task PublishOrderConfirmedAsync(OrderConfirmed message, CancellationToken ct = default)
        => await orderConfirmedProducer.Produce(message, ct);

    public async Task PublishOrderCancelledAsync(OrderCancelled message, CancellationToken ct = default)
        => await orderCancelledProducer.Produce(message, ct);
}