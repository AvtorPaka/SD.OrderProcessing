using Microsoft.Extensions.DependencyInjection;
using SD.OrderProcessing.Orders.Domain.Services;
using SD.OrderProcessing.Orders.Domain.Services.Interfaces;

namespace SD.OrderProcessing.Orders.Domain.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IOrdersService, OrdersService>();
        
        return services;
    }
}