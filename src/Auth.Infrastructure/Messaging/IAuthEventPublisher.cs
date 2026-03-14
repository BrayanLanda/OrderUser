using MassTransit;
using Shared.Contracts.Events.Auth;

namespace Auth.Infrastructure.Messaging;

public interface IAuthEventPublisher
{
    Task PublishUserValidatedAsync(UserValidated message, CancellationToken ct = default);
    Task PublishUserRejectedAsync(UserRejected message, CancellationToken ct = default);
}

public class AuthEventPublisher(
    ITopicProducer<UserValidated> validatedProducer,
    ITopicProducer<UserRejected> rejectedProducer) : IAuthEventPublisher
{
    public async Task PublishUserValidatedAsync(UserValidated message, CancellationToken ct = default)
        => await validatedProducer.Produce(message, ct);

    public async Task PublishUserRejectedAsync(UserRejected message, CancellationToken ct = default)
        => await rejectedProducer.Produce(message, ct);
}