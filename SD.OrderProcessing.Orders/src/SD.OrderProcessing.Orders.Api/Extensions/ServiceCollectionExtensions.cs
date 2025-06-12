using SD.OrderProcessing.Orders.Api.BackgroundServices;
using SD.OrderProcessing.Orders.Api.Filters;

namespace SD.OrderProcessing.Orders.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddGlobalFilters(this IServiceCollection services)
    {
        services.AddScoped<ExceptionFilter>();
        
        return services;
    }

    internal static IServiceCollection AddMessageQueueWorkers(this IServiceCollection services)
    {
        services.AddHostedService<OrderPaymentMessageProducer>();
        services.AddHostedService<PaymentStatusMessageConsumer>();
        
        return services;
    }
}