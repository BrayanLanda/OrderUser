namespace Products.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ProductsDbContext>(options => 
            options.UseNpgsql(configuration.GetConnectionString("ProductsDb")));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockReservationRepository, StockReservationRepository>();

        services.AddScoped<CreateProductUseCase>();
        services.AddScoped<GetProductsUseCase>();
        services.AddScoped<GetProductByIdUseCase>();

        services.AddMassTransit(x =>
        {
            x.UsingInMemory();

            x.AddRider(rider =>
            {
                rider.AddConsumer<OrderCreatedConsumer>();
                rider.AddConsumer<OrderCancelledConsumer>();

                rider.UsingKafka((ctx, k) =>
                {
                    k.Host(configuration["Kafka:BootstrapServers"]);

                    k.TopicEndpoint<OrderCreated>(
                        "order.created",
                        "products-service-group",
                        e => e.ConfigureConsumer<OrderCreatedConsumer>(ctx)
                    );

                    k.TopicEndpoint<OrderCancelled>(
                        "order.cancelled",
                        "products-service-group",
                        e => e.ConfigureConsumer<OrderCancelledConsumer>(ctx)
                    );
                });
            });
        });

        return services;
    }
}
