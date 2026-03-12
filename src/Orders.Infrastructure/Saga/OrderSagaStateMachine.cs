using MassTransit;
using Microsoft.Extensions.Logging;
using Orders.Domain.Repositories;
using Shared.Contracts.Events.Auth;
using Shared.Contracts.Events.Orders;
using Shared.Contracts.Events.Products;

namespace Orders.Infrastructure.Saga;

public class OrderSagaStateMachine : MassTransitStateMachine<OrderSagaState>
{
    private readonly ILogger<OrderSagaStateMachine> _logger;

    // Estados
    public State AwaitingUserValidation { get; private set; } = default!;
    public State Pending { get; private set; } = default!;
    public State Confirmed { get; private set; } = default!;
    public State Cancelled { get; private set; } = default!;

    // Eventos
    public Event<OrderValidationRequested> OrderValidationRequestedEvent { get; private set; } = default!;
    public Event<UserValidated> UserValidatedEvent { get; private set; } = default!;
    public Event<UserRejected> UserRejectedEvent { get; private set; } = default!;
    public Event<StockReserved> StockReservedEvent { get; private set; } = default!;
    public Event<StockInsufficient> StockInsufficientEvent { get; private set; } = default!;

    public OrderSagaStateMachine(ILogger<OrderSagaStateMachine> logger)
    {
        _logger = logger;

        // Estado inicial
        InstanceState(x => x.CurrentState);

        // Correlación — todos los eventos se unen por CorrelationId
        Event(() => OrderValidationRequestedEvent,
            x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => UserValidatedEvent,
            x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => UserRejectedEvent,
            x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => StockReservedEvent,
            x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => StockInsufficientEvent,
            x => x.CorrelateById(ctx => ctx.Message.CorrelationId));

        // ── FLUJO DE LA SAGA ──────────────────────────────────

        // 1. Saga inicia cuando Orders publica OrderValidationRequested
        Initially(
            When(OrderValidationRequestedEvent)
                .Then(ctx =>
                {
                    ctx.Saga.OrderId = ctx.Message.OrderId;
                    ctx.Saga.UserId = ctx.Message.UserId;
                    ctx.Saga.UserEmail = ctx.Message.UserEmail;
                    _logger.LogInformation("Saga iniciada para orden {OrderId}", ctx.Message.OrderId);
                })
                .TransitionTo(AwaitingUserValidation)
        );

        // 2. Auth validó el usuario → crear orden en BD y pedir stock
        During(AwaitingUserValidation,
            When(UserValidatedEvent)
                .ThenAsync(async ctx =>
                {
                    _logger.LogInformation("Usuario validado para orden {OrderId}", ctx.Saga.OrderId);

                    // Actualizar estado de la orden en BD
                    var orderRepo = ctx.GetPayload<IOrderRepository>();
                    var order = await orderRepo.GetByIdAsync(ctx.Saga.OrderId);
                    if (order is not null)
                    {
                        order.MarkAsPending();
                        await orderRepo.UpdateAsync(order);
                    }
                })
                .Publish(ctx => new OrderCreated
                {
                    CorrelationId = ctx.Saga.CorrelationId,
                    OrderId = ctx.Saga.OrderId,
                    UserId = ctx.Saga.UserId,
                    Items = ctx.Saga.Items.Select(i => new Shared.Contracts.Events.Orders.OrderItem
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                    }).ToList(),
                    TotalAmount = ctx.Saga.TotalAmount,
                })
                .TransitionTo(Pending),

            // Auth rechazó el usuario → cancelar
            When(UserRejectedEvent)
                .ThenAsync(async ctx =>
                {
                    _logger.LogWarning("Usuario rechazado para orden {OrderId}: {Reason}",
                        ctx.Saga.OrderId, ctx.Message.Reason);

                    var orderRepo = ctx.GetPayload<IOrderRepository>();
                    var order = await orderRepo.GetByIdAsync(ctx.Saga.OrderId);
                    if (order is not null)
                    {
                        order.Cancel(ctx.Message.Reason);
                        await orderRepo.UpdateAsync(order);
                    }
                })
                .Publish(ctx => new OrderCancelled
                {
                    CorrelationId = ctx.Saga.CorrelationId,
                    OrderId = ctx.Saga.OrderId,
                    UserId = ctx.Saga.UserId,
                    Reason = ctx.Message.Reason,
                    Items = ctx.Saga.Items.Select(i => new Shared.Contracts.Events.Orders.OrderItem
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                    }).ToList(),
                })
                .TransitionTo(Cancelled)
        );

        // 3. Products reservó el stock → confirmar orden
        During(Pending,
            When(StockReservedEvent)
                .ThenAsync(async ctx =>
                {
                    _logger.LogInformation("Stock reservado para orden {OrderId}", ctx.Saga.OrderId);

                    var orderRepo = ctx.GetPayload<IOrderRepository>();
                    var order = await orderRepo.GetByIdAsync(ctx.Saga.OrderId);
                    if (order is not null)
                    {
                        order.Confirm();
                        await orderRepo.UpdateAsync(order);
                    }
                })
                .Publish(ctx => new OrderConfirmed
                {
                    CorrelationId = ctx.Saga.CorrelationId,
                    OrderId = ctx.Saga.OrderId,
                    UserId = ctx.Saga.UserId,
                    TotalAmount = ctx.Saga.TotalAmount,
                })
                .TransitionTo(Confirmed),

            // Products no tiene stock → transacción compensatoria
            When(StockInsufficientEvent)
                .ThenAsync(async ctx =>
                {
                    var reason = $"Stock insuficiente para: {string.Join(", ", ctx.Message.Items.Select(i => i.ProductName))}";
                    _logger.LogWarning("Stock insuficiente para orden {OrderId}", ctx.Saga.OrderId);

                    var orderRepo = ctx.GetPayload<IOrderRepository>();
                    var order = await orderRepo.GetByIdAsync(ctx.Saga.OrderId);
                    if (order is not null)
                    {
                        order.Cancel(reason);
                        await orderRepo.UpdateAsync(order);
                    }
                })
                .Publish(ctx => new OrderCancelled
                {
                    CorrelationId = ctx.Saga.CorrelationId,
                    OrderId = ctx.Saga.OrderId,
                    UserId = ctx.Saga.UserId,
                    Reason = $"Stock insuficiente para: {string.Join(", ", ctx.Message.Items.Select(i => i.ProductName))}",
                    Items = ctx.Saga.Items.Select(i => new Shared.Contracts.Events.Orders.OrderItem
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                    }).ToList(),
                })
                .TransitionTo(Cancelled)
        );

        // Estados finales — la Saga termina aquí
        SetCompletedWhenFinalized();
    }
}


