namespace Orders.Application.DTOs;

public record OrderDto(
    Guid Id,
    Guid UserId,
    string UserEmail,
    string Status,
    decimal TotalAmount,
    List<OrderItemDto> Items,
    string? CancellationReason,
    DateTime CreatedAt
);
