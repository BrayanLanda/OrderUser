using Shared.Contracts.Events.Auth;

namespace Orders.Application.Services;

public interface IEventPublisher
{
    Task PublishOrderValidationRequestedAsync(OrderValidationRequested message, CancellationToken ct = default);
}