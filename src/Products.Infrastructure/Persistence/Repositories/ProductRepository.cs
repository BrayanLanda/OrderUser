using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;
using Products.Domain.Repositories;

namespace Products.Infrastructure.Persistence.Repositories
{
    public class ProductRepository(ProductsDbContext context) : IProductRepository
    {
        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Products.FindAsync([id], ct);

        public async Task<List<Product>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default) =>
            await context.Products.Where(p => ids.Contains(p.Id)).ToListAsync(ct);

        public async Task<List<Product>> GetAllAsync(CancellationToken ct = default) =>
            await context.Products.Where(p => p.IsActive).ToListAsync(ct);

        public async Task CreateAsync(Product product, CancellationToken ct = default)
        {
            context.Products.Add(product);
            await context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Product product, CancellationToken ct = default)
        {
            context.Products.Update(product);
            await context.SaveChangesAsync(ct);
        }
    }
}