using Auth.Application.Services;
using Auth.Application.UseCases;
using Auth.Domain.Repositories;
using Auth.Infrastructure.Messaging;
using Auth.Infrastructure.Messaging.Consumers;
using Auth.Infrastructure.Persistence;
using Auth.Infrastructure.Persistence.Repositories;
using Auth.Infrastructure.Security;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.Events.Auth;
using Shared.Contracts.Events.Orders;

namespace Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDB"));
        services.AddSingleton(sp =>
        {
            var settings = configuration.GetSection("MongoDB").Get<MongoDbSettings>()!;
            return new MongoDbContext(settings);
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        services.AddScoped<IAuthEventPublisher, AuthEventPublisher>();

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddScoped<IJwtService, JwtService>();

        services.AddScoped<RegisterUseCase>();
        services.AddScoped<LoginUseCase>();
        services.AddScoped<RefreshTokenUseCase>();

        services.AddMassTransit(x =>
        {
            x.UsingInMemory();
            x.AddRider(rider =>
            {
                rider.AddConsumer<OrderValidationRequestedConsumer>();
                rider.AddConsumer<OrderConfirmedConsumer>();
                rider.AddConsumer<OrderCancelledConsumer>();
                rider.AddProducer<UserValidated>("user.validated");
                rider.AddProducer<UserRejected>("user.rejected");

                rider.UsingKafka((ctx, k) =>
                {
                    k.Host(configuration["Kafka:BootstrapServers"]);

                    k.TopicEndpoint<OrderValidationRequested>(
                        "order.validation.requested",
                        "auth-service-group",
                        e => e.ConfigureConsumer<OrderValidationRequestedConsumer>(ctx)
                    );

                    k.TopicEndpoint<OrderConfirmed>(
                        "order.confirmed",
                        "auth-service-group",
                        e => e.ConfigureConsumer<OrderConfirmedConsumer>(ctx)
                    );

                    k.TopicEndpoint<OrderCancelled>(
                        "order.cancelled",
                        "auth-service-group",
                        e => e.ConfigureConsumer<OrderCancelledConsumer>(ctx)
                    );
                });
            });
        });
        return services;
    }
}