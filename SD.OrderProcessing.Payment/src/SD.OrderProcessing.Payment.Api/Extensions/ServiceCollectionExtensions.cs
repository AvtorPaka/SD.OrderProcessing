using SD.OrderProcessing.Payment.Api.Filters;

namespace SD.OrderProcessing.Payment.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddGlobalFilters(this IServiceCollection services)
    {
        services.AddScoped<ExceptionFilter>();

        return services;
    }

    internal static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        return services;
    }
}