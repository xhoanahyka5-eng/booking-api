using System.Security.Claims;
using System.Text.Json;
using Booking.Application.Abstractions.Logging;
using Booking.Application.Common.Exceptions;
using Booking.Application.Common.Logging;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IKafkaLogProducer _kafkaLogProducer;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IKafkaLogProducer kafkaLogProducer)
    {
        _next = next;
        _logger = logger;
        _kafkaLogProducer = kafkaLogProducer;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (title, statusCode) = GetProblemDetails(ex);

            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);

            await PublishExceptionLogAsync(context, ex, title, statusCode);
            await HandleExceptionAsync(context, ex, title, statusCode);
        }
    }

    private async Task PublishExceptionLogAsync(
        HttpContext context,
        Exception exception,
        string title,
        int statusCode)
    {
        try
        {
            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var level = GetLogLevel(exception);

            await _kafkaLogProducer.PublishAsync(new LogMessage
            {
                Level = level,
                Service = "Booking.Api",
                Message = $"{context.Request.Method} {context.Request.Path} failed: {exception.Message}",
                Exception = level == "Error" ? exception.ToString() : null,
                UserId = userId,
                TraceId = context.TraceIdentifier,
                CreatedAtUtc = DateTime.UtcNow
            }, CancellationToken.None);
        }
        catch (Exception kafkaEx)
        {
            _logger.LogError(kafkaEx, "Failed to publish exception log to Kafka.");
        }
    }

    private static string GetLogLevel(Exception exception)
    {
        return exception switch
        {
            ValidationException => "Warning",
            NotFoundException => "Warning",
            ConflictException => "Warning",
            UnauthorizedException => "Warning",
            AppException => "Warning",
            _ => "Error"
        };
    }

    private static (string Title, int StatusCode) GetProblemDetails(Exception exception)
    {
        return exception switch
        {
            ValidationException => ("Validation Error", StatusCodes.Status400BadRequest),
            NotFoundException => ("Not Found", StatusCodes.Status404NotFound),
            ConflictException => ("Conflict", StatusCodes.Status409Conflict),
            UnauthorizedException => ("Unauthorized", StatusCodes.Status401Unauthorized),
            AppException => ("Application Error", StatusCodes.Status400BadRequest),
            _ => ("Server Error", StatusCodes.Status500InternalServerError)
        };
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        string title,
        int statusCode)
    {
        var problem = new ProblemDetails
        {
            Title = title,
            Status = statusCode,
            Instance = context.Request.Path,
            Detail = exception.Message,
            Extensions =
            {
                ["traceId"] = context.TraceIdentifier
            }
        };

        if (exception is ValidationException validationException)
        {
            problem.Extensions["errors"] = validationException.Errors
                .Select(e => new
                {
                    e.PropertyName,
                    e.ErrorMessage
                })
                .ToList();
        }

        context.Response.StatusCode = problem.Status.Value;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(problem);
        await context.Response.WriteAsync(json);
    }
}