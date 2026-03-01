using Microsoft.AspNetCore.Builder;

namespace Booking.Api.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseCustomMiddlewares(
        this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();

        return app;
    }
}