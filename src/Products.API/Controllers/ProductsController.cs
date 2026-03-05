using Microsoft.AspNetCore.Mvc;
using Products.Application.DTOs;
using Products.Application.UseCases;

namespace Products.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(
    CreateProductUseCase createProduct,
    GetProductsUseCase getProducts,
    GetProductByIdUseCase getProductById) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var products = await getProducts.ExecuteAsync(ct);
        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var product = await getProductById.ExecuteAsync(id, ct);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var product = await createProduct.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }
}
