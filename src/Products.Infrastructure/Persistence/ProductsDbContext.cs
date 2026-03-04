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

            modelBuilder.Entity<Product>().HasData(
                CreateProduct(Guid.Parse("a1b2c3d4-0001-0000-0000-000000000001"), "Laptop Pro 15", "Laptop de alto rendimiento", 1500.00m, 10),
                CreateProduct(Guid.Parse("a1b2c3d4-0002-0000-0000-000000000002"), "Mouse Inalámbrico", "Mouse ergonómico inalámbrico", 35.00m, 50),
                CreateProduct(Guid.Parse("a1b2c3d4-0003-0000-0000-000000000003"), "Teclado Mecánico", "Teclado mecánico RGB", 120.00m, 25),
                CreateProduct(Guid.Parse("a1b2c3d4-0004-0000-0000-000000000004"), "Monitor 4K", "Monitor 27 pulgadas 4K", 450.00m, 5)
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