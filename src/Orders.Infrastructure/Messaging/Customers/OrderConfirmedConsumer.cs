
using MassTransit;
using Microsoft.Extensions.Logging;
using Orders.Domain.Repositories;
using Shared.Contracts.Events.Orders;

namespace Orders.Infrastructure.Messaging.Customers;

public class OrderConfirmedConsumer(
IOrderRepository orderRepo,
ILogger<OrderConfirmedConsumer> logger) : IConsumer<OrderConfirmed>
{
    public async Task Consume(ConsumeContext<OrderConfirmed> context)
    {
        var msg = context.Message;
        logger.LogInformation("Confirmando orden {OrderId} en BD", msg.OrderId);

        var order = await orderRepo.GetByIdAsync(msg.OrderId, context.CancellationToken);
        if (order is null) return;

        order.Confirm();
        await orderRepo.UpdateAsync(order, context.CancellationToken);

        logger.LogInformation("Orden {OrderId} confirmada en BD", msg.OrderId);
    }
}
