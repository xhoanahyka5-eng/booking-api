using System.Text.Json;
using FluentValidation;
using Booking.Application.Common.Exceptions;

namespace Booking.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled Exception");

            context.Response.ContentType = "application/json";

            // 1️⃣ FluentValidation (400)
            if (ex is ValidationException validationException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;

                await context.Response.WriteAsJsonAsync(new
                {
                    errors = validationException.Errors
                        .Select(e => e.ErrorMessage)
                });

                return;
            }

            // 2️⃣ Custom Business Exceptions
            if (ex is AppException appException)
            {
                context.Response.StatusCode = appException.StatusCode;

                await context.Response.WriteAsJsonAsync(new
                {
                    message = appException.Message
                });

                return;
            }

            // 3️⃣ Unexpected Errors (500)
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(new
            {
                message = "Internal Server Error"
            });
        }
    }
}