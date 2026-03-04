using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Products.Application.DTOs;
using Products.Domain.Repositories;

namespace Products.Application.UseCases
{
    public class GetProductByIdUseCase(IProductRepository productRepo)
    {
        public async Task<ProductDto?> ExecuteAsync(Guid id, CancellationToken ct = default)
        {
            var product = await productRepo.GetByIdAsync(id, ct);
            if (product is null) return null;
            return new ProductDto(product.Id, product.Name, product.Description, product.Price, product.Stock, product.IsActive);
        }
    }
}