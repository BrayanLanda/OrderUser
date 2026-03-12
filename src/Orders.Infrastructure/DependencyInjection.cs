using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Application.UsesCases;
using Orders.Domain.Repositories;
using Orders.Infrastructure.Persistence;
using Orders.Infrastructure.Persistence.Repositories;
using Orders.Infrastructure.Saga;
using Shared.Contracts.Events.Auth;
using Shared.Contracts.Events.Orders;
using Shared.Contracts.Events.Products;

namespace Orders.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── EF Core + PostgreSQL ──────────────────────────────
        services.AddDbContext<OrdersDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("OrdersDb")));

        // ── Repositorios ──────────────────────────────────────
        services.AddScoped<IOrderRepository, OrderRepository>();

        // ── Casos de uso ──────────────────────────────────────
        services.AddScoped<CreateOrderUseCase>();
        services.AddScoped<GetOrderByIdUseCase>();
        services.AddScoped<GetOrdersByUserUseCase>();

        // ── MassTransit + Kafka + Saga ────────────────────────
        services.AddMassTransit(x =>
        {
            x.UsingInMemory();

            // Registrar la Saga con persistencia en EF Core
            x.AddSagaStateMachine<OrderSagaStateMachine, OrderSagaState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                    r.AddDbContext<DbContext, OrdersDbContext>((sp, opts) =>
                        opts.UseNpgsql(configuration.GetConnectionString("OrdersDb")));
                    r.UsePostgres();
                });

            x.AddRider(rider =>
            {
                // Producers — tópicos que Orders publica
                rider.AddProducer<OrderValidationRequested>("order.validation.requested");
                rider.AddProducer<OrderCreated>("order.created");
                rider.AddProducer<OrderConfirmed>("order.confirmed");
                rider.AddProducer<OrderCancelled>("order.cancelled");

                // Saga consumers — tópicos que la Saga consume
                rider.AddSagaStateMachine<OrderSagaStateMachine, OrderSagaState>()
                    .EntityFrameworkRepository(r =>
                    {
                        r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                        r.AddDbContext<DbContext, OrdersDbContext>((sp, opts) =>
                            opts.UseNpgsql(configuration.GetConnectionString("OrdersDb")));
                        r.UsePostgres();
                    });

                rider.UsingKafka((ctx, k) =>
                {
                    k.Host(configuration["Kafka:BootstrapServers"]);

                    // La Saga escucha estos tópicos
                    k.TopicEndpoint<UserValidated>(
                        "user.validated",
                        "orders-saga-group",
                        e => e.ConfigureSaga<OrderSagaState>(ctx)
                    );

                    k.TopicEndpoint<UserRejected>(
                        "user.rejected",
                        "orders-saga-group",
                        e => e.ConfigureSaga<OrderSagaState>(ctx)
                    );

                    k.TopicEndpoint<StockReserved>(
                        "stock.reserved",
                        "orders-saga-group",
                        e => e.ConfigureSaga<OrderSagaState>(ctx)
                    );

                    k.TopicEndpoint<StockInsufficient>(
                        "stock.insufficient",
                        "orders-saga-group",
                        e => e.ConfigureSaga<OrderSagaState>(ctx)
                    );

                    // OrderValidationRequested también inicia la Saga
                    k.TopicEndpoint<OrderValidationRequested>(
                        "order.validation.requested",
                        "orders-saga-init-group",
                        e => e.ConfigureSaga<OrderSagaState>(ctx)
                    );
                });
            });
        });

        return services;
    }
}
