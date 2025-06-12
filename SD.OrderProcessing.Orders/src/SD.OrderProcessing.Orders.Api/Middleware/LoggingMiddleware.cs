using System.Text;
using SD.OrderProcessing.Orders.Api.Extensions;

namespace SD.OrderProcessing.Orders.Api.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILogger<LoggingMiddleware> logger,
        IWebHostEnvironment hostEnvironment)
    {
        string callId = context.TraceIdentifier;

        logger.LogRequestStart(
            callId: callId,
            curTime: DateTime.UtcNow,
            endpointRoute: context.Request.Path
        );
        
        if (hostEnvironment.IsDevelopment())
        {
            logger.LogRequestHeaders(
                curTime: DateTime.UtcNow,
                callId: callId,
                headers: QueryRequestHeader(context.Request)
            );
        }

        await _next.Invoke(context);

        logger.LogRequestEnd(
            callId: callId,
            curTime: DateTime.UtcNow,
            endpointRoute: context.Request.Path
        );
    }
    
    private string QueryRequestHeader(HttpRequest request)
    {
        StringBuilder headerMetaBuilder = new StringBuilder();

        foreach (var headerMeta in request.Headers)
        {
            headerMetaBuilder.Append($">>header: {headerMeta.Key}, value: {headerMeta.Value}\n");
        }

        return headerMetaBuilder.ToString();
    }
}