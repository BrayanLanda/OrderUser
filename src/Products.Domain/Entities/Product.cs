namespace Products.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public Product() { }

    public static Product Create(string name, string description, decimal price, int stock)
    {
       if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("The name is required");
        if (price <= 0)
            throw new ArgumentException("The price must be greater than zero");
        if (stock < 0)
            throw new ArgumentException("Stock cannot be negative");

        return new Product
        {
            Name = name.Trim(),
            Description = description.Trim(),
            Price = price,
            Stock = stock,
        };
    }

    public bool HasStock(int quantity) => Stock >= quantity;

    public void ReserveStock(int quantity)
    {
        if (!HasStock(quantity))
            throw new InvalidOperationException($"Insufficient stock. Available: {Stock}, requested: {quantity}.");

        Stock -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseStock(int quantity)
    {
        Stock += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new ArgumentException("The price must be greater than zero");
        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }
}