using Microsoft.Extensions.DependencyInjection;
using SD.OrderProcessing.Payment.Domain.Services;
using SD.OrderProcessing.Payment.Domain.Services.Interfaces;

namespace SD.OrderProcessing.Payment.Domain.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IBalanceAccountsService, BalanceAccountService>();
        
        return services;
    }
}