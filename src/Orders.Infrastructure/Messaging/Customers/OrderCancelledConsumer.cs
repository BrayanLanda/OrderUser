using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Orders.Domain.Repositories;
using Shared.Contracts.Events.Orders;

namespace Orders.Infrastructure.Messaging.Customers;

public class OrderCancelledConsumer(
    IOrderRepository orderRepo,
    ILogger<OrderCancelledConsumer> logger) : IConsumer<OrderCancelled>
{
    public async Task Consume(ConsumeContext<OrderCancelled> context)
    {
        var msg = context.Message;
        logger.LogInformation("Cancelando orden {OrderId} en BD", msg.OrderId);

        var order = await orderRepo.GetByIdAsync(msg.OrderId, context.CancellationToken);
        if (order is null) return;

        order.Cancel(msg.Reason);
        await orderRepo.UpdateAsync(order, context.CancellationToken);

        logger.LogInformation("Orden {OrderId} cancelada en BD", msg.OrderId);
    }
}
