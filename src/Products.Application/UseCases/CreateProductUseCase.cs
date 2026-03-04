using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Products.Application.DTOs;
using Products.Domain.Entities;
using Products.Domain.Repositories;

namespace Products.Application.UseCases
{
    public class CreateProductUseCase(IProductRepository productRepo)
    {
        public async Task<ProductDto> ExecuteAsync(CreateProductRequest request, CancellationToken ct = default)
        {
            var product = Product.Create(request.Name, request.Description, request.Price, request.Stock);
            await productRepo.CreateAsync(product, ct);
            return MapToDto(product);
        }
        private static ProductDto MapToDto(Product p) =>
            new(p.Id, p.Name, p.Description, p.Price, p.Stock, p.IsActive);
    }
}
