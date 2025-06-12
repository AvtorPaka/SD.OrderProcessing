namespace SD.OrderProcessing.Orders.Api.Middleware;

public class TracingMiddleware
{
    private const string TraceIdHeaderKey = "X-Trace-Id";
    private readonly RequestDelegate _next;

    public TracingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string? givenTraceId = context.Request.Headers[TraceIdHeaderKey];
        context.TraceIdentifier = string.IsNullOrEmpty(givenTraceId) ? Guid.NewGuid().ToString() : givenTraceId;
        context.Response.Headers[TraceIdHeaderKey] = context.TraceIdentifier;
        await _next.Invoke(context);
    }
}