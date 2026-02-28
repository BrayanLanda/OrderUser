namespace Auth.Domain.Entities.Enums;

public enum ActivityType
{
    Login,
    Logout,
    OrderConfirmed,
    OrderCancelled,
    PasswordChanged,
    AccountDeactivated,
}