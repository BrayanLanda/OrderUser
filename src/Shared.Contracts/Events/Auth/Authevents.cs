namespace Shared.Contracts.Events.Auth;

public record OrderValidationRequested
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = default!;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public record UserValidated
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = default!;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public record UserRejected
{
    public Guid CorrelationId { get; init; }
    public Guid OrderId       { get; init; }
    public Guid UserId        { get; init; }
    public string Reason      { get; init; } = default!;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}