using System.Text.Json;
using System.Text.Json.Serialization;

namespace SD.OrderProcessing.Orders.Api;

internal sealed class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _hostEnvironment;

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _hostEnvironment = environment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .AddMvcOptions(options =>
            {
                // options.Filters.Add()
            });
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();


        app.UseEndpoints(builder =>
        {
            builder.MapControllers();
        });
    }
}