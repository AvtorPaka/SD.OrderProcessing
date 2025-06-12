using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SD.OrderProcessing.Orders.Domain.Contracts.Dal.Interfaces;
using SD.OrderProcessing.Orders.Infrastructure.Configuration.Options;
using SD.OrderProcessing.Orders.Infrastructure.Dal.Infrastructure;
using SD.OrderProcessing.Orders.Infrastructure.Dal.Repositories;

namespace SD.OrderProcessing.Orders.Infrastructure.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDalInfrastructure(this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment)
    {
        var postgreConnectionSection =
            configuration.GetSection($"Infrastructure:Dal:{nameof(PostgreConnectionOptions)}");

        PostgreConnectionOptions pgConnectionOptions = postgreConnectionSection.Get<PostgreConnectionOptions>() ??
                                                       throw new ArgumentException(
                                                           "PostgreSQL connection options are missing");

        Postgres.ConfigureTypeMapOptions();
        Postgres.AddDataSource(
            services: services,
            connectionOptions: pgConnectionOptions,
            isDevelopment: isDevelopment
        );
        Postgres.AddMigrations(
            services: services,
            connectionOptions: pgConnectionOptions
        );

        return services;
    }

    public static IServiceCollection AddInfrastructureConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqConnectionOptions>(
            configuration.GetSection($"Infrastructure:ISC:{nameof(RabbitMqConnectionOptions)}"));

        return services;
    }

    public static IServiceCollection AddDalRepositories(this IServiceCollection services)
    {
        services.AddScoped<IOrdersRepository, OrdersRepository>();
        services.AddScoped<IOrderPaymentMessagesRepository, OrderPaymentMessagesRepository>();

        return services;
    }
}