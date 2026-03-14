using MassTransit.KafkaIntegration;
using Orders.Application.DTOs;
using Orders.Application.Services;
using Orders.Domain.Entities;
using Orders.Domain.Repositories;
using Shared.Contracts.Events.Auth;

namespace Orders.Application.UsesCases;

public class CreateOrderUseCase(
    IOrderRepository orderRepo,
    IEventPublisher eventPublisher)
{
    public async Task<OrderDto> ExecuteAsync(
        CreateOrderRequest request,
        Guid userId,
        string userEmail,
        CancellationToken ct = default)
    {
        var items = request.Items.Select(i =>
            OrderItem.Create(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)
        ).ToList();

        var order = Order.Create(userId, userEmail, items);
        await orderRepo.CreateAsync(order, ct);

        await eventPublisher.PublishOrderValidationRequestedAsync(new OrderValidationRequested
        {
            CorrelationId = order.Id,
            OrderId = order.Id,
            UserId = userId,
            UserEmail = userEmail,
            Items = order.Items.Select(i => new OrderValidationItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
            }).ToList(),
        }, ct);
        return MapToDto(order);
    }

    private static OrderDto MapToDto(Order o) => new(
        o.Id, o.UserId, o.UserEmail, o.Status.ToString(),
        o.TotalAmount,
        o.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList(),
        o.CancellationReason, o.CreatedAt
    );
}
