using Products.Domain.Entities;

namespace Products.Domain.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<Product>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default);
        Task<List<Product>> GetAllAsync(CancellationToken ct = default);
        Task CreateAsync(Product product, CancellationToken ct = default);
        Task UpdateAsync(Product product, CancellationToken ct = default);
    }
}