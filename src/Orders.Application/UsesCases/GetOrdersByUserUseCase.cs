using Orders.Application.DTOs;
using Orders.Domain.Repositories;

namespace Orders.Application.UsesCases;

public class GetOrdersByUserUseCase(IOrderRepository orderRepo)
{
    public async Task<List<OrderDto>> ExecuteAsync(Guid userId, CancellationToken ct = default)
    {
        var orders = await orderRepo.GetByUserIdAsync(userId, ct);
        return orders.Select(o => new OrderDto(
            o.Id, o.UserId, o.UserEmail, o.Status.ToString(),
            o.TotalAmount,
            o.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList(),
            o.CancellationReason, o.CreatedAt
        )).ToList();
    }
}
