using System.Text.Json;
using Booking.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

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

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var problem = new ProblemDetails
        {
            Instance = context.Request.Path,
            Detail = exception.Message,
            Extensions =
            {
                ["traceId"] = context.TraceIdentifier
            }
        };

        switch (exception)
        {
            case ValidationException validationException:
                problem.Title = "Validation Error";
                problem.Status = StatusCodes.Status400BadRequest;
                problem.Extensions["errors"] = validationException.Errors
                    .Select(e => new
                    {
                        e.PropertyName,
                        e.ErrorMessage
                    })
                    .ToList();
                break;

            case NotFoundException:
                problem.Title = "Not Found";
                problem.Status = StatusCodes.Status404NotFound;
                break;

            case ConflictException:
                problem.Title = "Conflict";
                problem.Status = StatusCodes.Status409Conflict;
                break;

            case UnauthorizedException:
                problem.Title = "Unauthorized";
                problem.Status = StatusCodes.Status401Unauthorized;
                break;

            case AppException:
                problem.Title = "Application Error";
                problem.Status = StatusCodes.Status400BadRequest;
                break;

            default:
                problem.Title = "Server Error";
                problem.Status = StatusCodes.Status500InternalServerError;
                break;
        }

        context.Response.StatusCode = problem.Status!.Value;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(problem);
        await context.Response.WriteAsync(json);
    }
}