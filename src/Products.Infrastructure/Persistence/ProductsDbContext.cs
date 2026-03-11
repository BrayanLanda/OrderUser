using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;

namespace Products.Infrastructure.Persistence
{
    public class ProductsDbContext(DbContextOptions<ProductsDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products => Set<Product>();
        public DbSet<StockReservation> StockReservations => Set<StockReservation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(e =>
                {
                    e.HasKey(p => p.Id);
                    e.Property(p => p.Name).IsRequired().HasMaxLength(200);
                    e.Property(p => p.Description).HasMaxLength(1000);
                    e.Property(p => p.Price).HasPrecision(18, 2);
                    e.Property(p => p.Stock).IsRequired();
                    e.HasIndex(p => p.Name);
                });

            modelBuilder.Entity<StockReservation>(e =>
            {
                e.HasKey(r => r.Id);
                e.Property(r => r.Status).HasConversion<string>();
                e.HasIndex(r => r.OrderId);
                e.HasIndex(r => new { r.OrderId, r.ProductId });
            });

            var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<Product>().HasData(
                new { Id = Guid.Parse("a1b2c3d4-0001-0000-0000-000000000001"), Name = "Laptop Pro 15", Description = "Laptop de alto rendimiento", Price = 1500.00m, Stock = 10, IsActive = true, CreatedAt = createdAt, UpdatedAt = createdAt },
                new { Id = Guid.Parse("a1b2c3d4-0002-0000-0000-000000000002"), Name = "Mouse Inalámbrico", Description = "Mouse ergonómico inalámbrico", Price = 35.00m, Stock = 50, IsActive = true, CreatedAt = createdAt, UpdatedAt = createdAt },
                new { Id = Guid.Parse("a1b2c3d4-0003-0000-0000-000000000003"), Name = "Teclado Mecánico", Description = "Teclado mecánico RGB", Price = 120.00m, Stock = 25, IsActive = true, CreatedAt = createdAt, UpdatedAt = createdAt },
                new { Id = Guid.Parse("a1b2c3d4-0004-0000-0000-000000000004"), Name = "Monitor 4K", Description = "Monitor 27 pulgadas 4K", Price = 450.00m, Stock = 5, IsActive = true, CreatedAt = createdAt, UpdatedAt = createdAt }
            );
        }

        private static Product CreateProduct(Guid id, string name, string desc, decimal price, int stock)
        {
            var product = Product.Create(name, desc, price, stock);
            typeof(Product).GetProperty("Id")!.SetValue(product, id);
            return product;
        }

    }
}