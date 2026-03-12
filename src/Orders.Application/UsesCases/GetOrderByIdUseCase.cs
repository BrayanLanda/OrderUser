using Orders.Application.DTOs;
using Orders.Domain.Repositories;

namespace Orders.Application.UsesCases;

public class GetOrderByIdUseCase(IOrderRepository orderRepo)
{
    public async Task<OrderDto?> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orderRepo.GetByIdAsync(id, ct);
        if (order is null) return null;

        return new OrderDto(
            order.Id, order.UserId, order.UserEmail, order.Status.ToString(),
            order.TotalAmount,
            order.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList(),
            order.CancellationReason, order.CreatedAt
        );
    }
}
