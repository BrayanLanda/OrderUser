using Orders.Application.DTOs;
using Orders.Domain.Entities;
using Orders.Domain.Repositories;

namespace Orders.Application.UsesCases;

public class CreateOrderUseCase(
    IOrderRepository orderRepo,
    IPublishEndpoint publishEndpoint)
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

        await publishEndpoint.Publish(new OrderValidationRequested
        {
            CorrelationId = order.Id,
            OrderId = order.Id,
            UserId = userId,
            UserEmail = userEmail,
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
