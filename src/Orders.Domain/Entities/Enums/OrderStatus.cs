namespace Orders.Domain.Entities.Enums;

public enum OrderStatus
{
    AwaitingUserValidation,
    Pending,
    Confirmed,
    Cancelled
}