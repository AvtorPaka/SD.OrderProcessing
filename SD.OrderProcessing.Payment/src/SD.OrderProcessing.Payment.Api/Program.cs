using System.Net;
using SD.OrderProcessing.Payment.Infrastructure.DependencyInjection.Extensions;

namespace SD.OrderProcessing.Payment.Api;

public sealed class Program
{
    public static async Task Main()
    {
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.ConfigureKestrel((context, serverOptions) =>
                {
                    serverOptions.Listen(IPAddress.Any, 7070);
                });
            });

        await hostBuilder
            .Build()
            .MigrateUp()
            .RunAsync();
    }
}