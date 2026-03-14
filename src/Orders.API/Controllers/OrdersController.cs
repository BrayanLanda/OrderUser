using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.DTOs;
using Orders.Application.UsesCases;

namespace Orders.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController(
    CreateOrderUseCase createOrder,
    GetOrderByIdUseCase getOrderById,
    GetOrdersByUserUseCase getOrdersByUser) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Token inválido."));

        var userEmail = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("email")
            ?? string.Empty;

        var order = await createOrder.ExecuteAsync(request, userId, userEmail, ct);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var order = await getOrderById.ExecuteAsync(id, ct);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Token inválido."));

        var orders = await getOrdersByUser.ExecuteAsync(userId, ct);
        return Ok(orders);
    }
}