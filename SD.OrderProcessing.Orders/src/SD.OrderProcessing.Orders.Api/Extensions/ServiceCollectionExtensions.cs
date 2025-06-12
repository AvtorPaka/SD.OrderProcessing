using SD.OrderProcessing.Orders.Api.Filters;

namespace SD.OrderProcessing.Orders.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddGlobalFilters(this IServiceCollection services)
    {
        services.AddScoped<ExceptionFilter>();
        
        return services;
    }
}