using System.Text.Json;
using System.Text.Json.Serialization;
using SD.OrderProcessing.Payment.Api.Extensions;
using SD.OrderProcessing.Payment.Api.Filters;
using SD.OrderProcessing.Payment.Api.Middleware;
using SD.OrderProcessing.Payment.Domain.DependencyInjection.Extensions;
using SD.OrderProcessing.Payment.Infrastructure.DependencyInjection.Extensions;

namespace SD.OrderProcessing.Payment.Api;

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
            .AddGlobalFilters()
            .AddMessageQueueWorkers()
            .AddDomainServices()
            .AddInfrastructureConfiguration(
                configuration: _configuration
            )
            .AddDalInfrastructure(
                configuration: _configuration,
                isDevelopment: _hostEnvironment.IsDevelopment()
            )
            .AddDalRepositories()
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .AddMvcOptions(options =>
            {
                options.Filters.Add<ExceptionFilter>();
            });
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseMiddleware<TracingMiddleware>();
        app.UseRouting();

        app.UseMiddleware<LoggingMiddleware>();

        app.UseEndpoints(builder =>
        {
            builder.MapControllers();
        });
    }
}