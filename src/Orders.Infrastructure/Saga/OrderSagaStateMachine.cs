using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orders.Infrastructure.Messaging;
using Shared.Contracts.Events.Auth;
using Shared.Contracts.Events.Orders;
using Shared.Contracts.Events.Products;

namespace Orders.Infrastructure.Saga;

public class OrderSagaStateMachine : MassTransitStateMachine<OrderSagaState>
{
    private readonly ILogger<OrderSagaStateMachine> _logger;
    private readonly IServiceProvider _serviceProvider;
    public State AwaitingUserValidation { get; private set; } = default!;
    public State Pending { get; private set; } = default!;
    public State Confirmed { get; private set; } = default!;
    public State Cancelled { get; private set; } = default!;

    public Event<OrderValidationRequested> OrderValidationRequestedEvent { get; private set; } = default!;
    public Event<UserValidated> UserValidatedEvent { get; private set; } = default!;
    public Event<UserRejected> UserRejectedEvent { get; private set; } = default!;
    public Event<StockReserved> StockReservedEvent { get; private set; } = default!;
    public Event<StockInsufficient> StockInsufficientEvent { get; private set; } = default!;

    public OrderSagaStateMachine(
        ILogger<OrderSagaStateMachine> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        InstanceState(x => x.CurrentState);

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

        Initially(
            When(OrderValidationRequestedEvent)
                .Then(ctx =>
        {
            ctx.Saga.OrderId = ctx.Message.OrderId;
            ctx.Saga.UserId = ctx.Message.UserId;
            ctx.Saga.UserEmail = ctx.Message.UserEmail;
            ctx.Saga.TotalAmount = ctx.Message.TotalAmount;
            ctx.Saga.Items = ctx.Message.Items.Select(i => new SagaOrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
            }).ToList();
            _logger.LogInformation("Saga iniciada para orden {OrderId}", ctx.Message.OrderId);
        })
        .TransitionTo(AwaitingUserValidation)
        );

        During(AwaitingUserValidation,
            When(UserValidatedEvent)
                .Then(ctx => _logger.LogInformation("Usuario validado para orden {OrderId}", ctx.Saga.OrderId))
                .ThenAsync(async ctx =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var publisher = scope.ServiceProvider.GetRequiredService<IOrderEventPublisher>();
                    await publisher.PublishOrderCreatedAsync(new OrderCreated
                    {
                        CorrelationId = ctx.Saga.CorrelationId,
                        OrderId = ctx.Saga.OrderId,
                        UserId = ctx.Saga.UserId,
                        TotalAmount = ctx.Saga.TotalAmount,
                        Items = ctx.Saga.Items.Select(i => new Shared.Contracts.Events.Orders.OrderItem
                        {
                            ProductId = i.ProductId,
                            ProductName = i.ProductName,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice,
                        }).ToList(),
                    });
                })
                .TransitionTo(Pending),

            When(UserRejectedEvent)
                .Then(ctx => _logger.LogWarning("Usuario rechazado para orden {OrderId}: {Reason}",
                    ctx.Saga.OrderId, ctx.Message.Reason))
                .ThenAsync(async ctx =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var publisher = scope.ServiceProvider.GetRequiredService<IOrderEventPublisher>();
                    await publisher.PublishOrderCancelledAsync(new OrderCancelled
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
                    });
                })
                .TransitionTo(Cancelled)
        );

        During(Pending,
            When(StockReservedEvent)
                .Then(ctx => _logger.LogInformation("Stock reservado para orden {OrderId}", ctx.Saga.OrderId))
                .ThenAsync(async ctx =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var publisher = scope.ServiceProvider.GetRequiredService<IOrderEventPublisher>();
                    await publisher.PublishOrderConfirmedAsync(new OrderConfirmed
                    {
                        CorrelationId = ctx.Saga.CorrelationId,
                        OrderId = ctx.Saga.OrderId,
                        UserId = ctx.Saga.UserId,
                        TotalAmount = ctx.Saga.TotalAmount,
                    });
                })
                .TransitionTo(Confirmed),

            When(StockInsufficientEvent)
                .Then(ctx => _logger.LogWarning("Stock insuficiente para orden {OrderId}", ctx.Saga.OrderId))
                .ThenAsync(async ctx =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var publisher = scope.ServiceProvider.GetRequiredService<IOrderEventPublisher>();
                    await publisher.PublishOrderCancelledAsync(new OrderCancelled
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
                    });
                })
                .TransitionTo(Cancelled)
        );

        SetCompletedWhenFinalized();
    }
}