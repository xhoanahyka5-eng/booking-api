using System.Diagnostics;

namespace Booking.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Incoming Request: {Method} {Path}",
            context.Request.Method,
            context.Request.Path
        );

        await _next(context);

        stopwatch.Stop();

        _logger.LogInformation(
            "Outgoing Response: {StatusCode} in {Elapsed}ms",
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds
        );
    }
}