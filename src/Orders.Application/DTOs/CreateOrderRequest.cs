namespace Orders.Application.DTOs;

public record CreateOrderRequest(
    List<OrderItemRequest> Items
);
