namespace Shared.Contracts.Events.Products;

public record StockReserved
{
    public Guid CorrelationId         { get; init; }
    public Guid OrderId               { get; init; }
    public List<ReservedItem> Items   { get; init; } = [];
    public DateTime Timestamp         { get; init; } = DateTime.UtcNow;
}

public record StockInsufficient
{
    public Guid CorrelationId              { get; init; }
    public Guid OrderId                    { get; init; }
    public List<InsufficientItem> Items    { get; init; } = [];
    public DateTime Timestamp              { get; init; } = DateTime.UtcNow;
}

public record ReservedItem
{
    public Guid ProductId    { get; init; }
    public int QuantityReserved { get; init; }
}

public record InsufficientItem
{
    public Guid ProductId       { get; init; }
    public string ProductName   { get; init; } = default!;
    public int Requested        { get; init; }
    public int Available        { get; init; }
}