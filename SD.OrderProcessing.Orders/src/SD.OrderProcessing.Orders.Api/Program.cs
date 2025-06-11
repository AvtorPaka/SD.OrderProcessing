using System.Net;

namespace SD.OrderProcessing.Orders.Api;

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
                    serverOptions.Listen(IPAddress.Any, 7077);
                });
            });

        await hostBuilder
            .Build()
            .RunAsync();
    }
}