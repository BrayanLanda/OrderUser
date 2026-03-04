using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Products.Application.DTOs;
using Products.Domain.Repositories;

namespace Products.Application.UseCases
{
    public class GetProductsUseCase(IProductRepository productRepo)
    {
        public async Task<List<ProductDto>> ExecuteAsync(CancellationToken ct = default)
        {
            var products = await productRepo.GetAllAsync(ct);
            return products.Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.Stock, p.IsActive)).ToList();
        }
    }
}