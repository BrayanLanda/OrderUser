using MassTransit;
using Orders.Application.Services;
using Shared.Contracts.Events.Auth;

namespace Orders.Infrastructure.Messaging;

public class EventPublisher(ITopicProducer<OrderValidationRequested> producer) : IEventPublisher
{
    public async Task PublishOrderValidationRequestedAsync(OrderValidationRequested message, CancellationToken ct = default)
        => await producer.Produce(message, ct);
}