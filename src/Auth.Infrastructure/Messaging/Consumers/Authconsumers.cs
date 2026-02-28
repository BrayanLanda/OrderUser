using Auth.Domain.Entities;
using Auth.Domain.Entities.Enums;
using Auth.Domain.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events.Auth;
using Shared.Contracts.Events.Orders;

namespace Auth.Infrastructure.Messaging.Consumers;

public class OrderValidationRequestedConsumer(
    IUserRepository userRepo,
    IPublishEndpoint publishEndpoint,
    ILogger<OrderValidationRequestedConsumer> logger)
    : IConsumer<OrderValidationRequested>
{
    public async Task Consume(ConsumeContext<OrderValidationRequested> context)
    {
        var msg = context.Message;
        logger.LogInformation("Validando usuario {UserId} para orden {OrderId}", msg.UserId, msg.OrderId);

        var user = await userRepo.GetByExternalIdAsync(msg.UserId, context.CancellationToken);

        if (user is null || !user.IsActive)
        {
            var reason = user is null ? "Usuario no encontrado." : "La cuenta está desactivada.";
            logger.LogWarning("Usuario {UserId} rechazado: {Reason}", msg.UserId, reason);

            await publishEndpoint.Publish(new UserRejected
            {
                CorrelationId = msg.CorrelationId,
                OrderId = msg.OrderId,
                UserId = msg.UserId,
                Reason = reason,
            }, context.CancellationToken);

            return;
        }

        logger.LogInformation("Usuario {UserId} validado correctamente", msg.UserId);

        await publishEndpoint.Publish(new UserValidated
        {
            CorrelationId = msg.CorrelationId,
            OrderId = msg.OrderId,
            UserId = msg.UserId,
            UserEmail = user.Email,
        }, context.CancellationToken);
    }
}

public class OrderConfirmedConsumer(
    IUserRepository userRepo,
    IActivityLogRepository activityLogRepo,
    ILogger<OrderConfirmedConsumer> logger)
    : IConsumer<OrderConfirmed>
{
    public async Task Consume(ConsumeContext<OrderConfirmed> context)
    {
        var msg = context.Message;
        logger.LogInformation("Registrando orden confirmada {OrderId} para usuario {UserId}", msg.OrderId, msg.UserId);

        var user = await userRepo.GetByExternalIdAsync(msg.UserId, context.CancellationToken);
        if (user is null) return;

        var log = ActivityLog.Create(
            userId: user.Id,
            type: ActivityType.OrderConfirmed,
            description: $"Orden {msg.OrderId} confirmada por ${msg.TotalAmount:F2}",
            metadata: new Dictionary<string, object>
            {
                { "orderId", msg.OrderId.ToString() },
                { "totalAmount", msg.TotalAmount },
                { "correlationId", msg.CorrelationId.ToString() }
            }
        );

        await activityLogRepo.CreateAsync(log, context.CancellationToken);
    }
}

public class OrderCancelledConsumer(
    IUserRepository userRepo,
    IActivityLogRepository activityLogRepo,
    ILogger<OrderCancelledConsumer> logger)
    : IConsumer<OrderCancelled>
{
    public async Task Consume(ConsumeContext<OrderCancelled> context)
    {
        var msg = context.Message;
        logger.LogInformation("Registrando orden cancelada {OrderId} para usuario {UserId}", msg.OrderId, msg.UserId);

        var user = await userRepo.GetByExternalIdAsync(msg.UserId, context.CancellationToken);
        if (user is null) return;

        var log = ActivityLog.Create(
            userId: user.Id,
            type: ActivityType.OrderCancelled,
            description: $"Orden {msg.OrderId} cancelada: {msg.Reason}",
            metadata: new Dictionary<string, object>
            {
                { "orderId", msg.OrderId.ToString() },
                { "reason", msg.Reason },
                { "correlationId", msg.CorrelationId.ToString() }
            }
        );

        await activityLogRepo.CreateAsync(log, context.CancellationToken);
    }
}