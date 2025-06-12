using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SD.OrderProcessing.Payment.Infrastructure.DependencyInjection.Extensions;

public static class HostExtensions
{
    public static IHost MigrateUp(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();

        return host;
    }
    
    public static IHost MigrateDown(this IHost host, long version = 20250612)
    {
        using var scope = host.Services.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateDown(version);

        return host;
    }
}