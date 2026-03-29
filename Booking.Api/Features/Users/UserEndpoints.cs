using Booking.Application.Features.Users.BecomeHost;
using Booking.Application.Features.Users.GetAllUsers;
using Booking.Application.Features.Users.Login;
using Booking.Application.Features.Users.Logout;
using Booking.Application.Features.Users.Refresh;
using Booking.Application.Features.Users.Register;
using MediatR;
using System.Security.Claims;

namespace Booking.Api.Features.Users;

public sealed record LogoutRequest(string? RefreshToken);

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/users/register",
            async (
                RegisterUserCommand command,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var result = await sender.Send(command, ct);
                return Results.Ok(result);
            });

        app.MapPost("/api/v1/users/login",
            async (
                LoginUserCommand command,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var result = await sender.Send(command, ct);
                return Results.Ok(result);
            });

        app.MapGet("/api/v1/users",
            async (
                HttpContext http,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var userIdStr =
                    http.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    http.User.FindFirst("sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdStr) ||
                    !Guid.TryParse(userIdStr, out var userId))
                {
                    return Results.Unauthorized();
                }

                var query = new GetAllUsersQuery(userId);
                var result = await sender.Send(query, ct);

                return Results.Ok(result);
            })
            .RequireAuthorization();

        app.MapPost("/api/v1/users/refresh",
            async (
                RefreshTokenCommand command,
                ISender sender
            ) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(result);
            });

        app.MapPost("/api/v1/users/logout",
            async (
                HttpContext http,
                LogoutRequest? body,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var userIdStr =
                    http.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    http.User.FindFirst("sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdStr) ||
                    !Guid.TryParse(userIdStr, out var userId))
                {
                    return Results.Unauthorized();
                }

                await sender.Send(new LogoutUserCommand(userId, body?.RefreshToken), ct);
                return Results.Ok(new { message = "Logged out." });
            })
            .RequireAuthorization();

        app.MapPost("/api/v1/users/become-host",
            async (
                BecomeHostCommand command,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var result = await sender.Send(command, ct);
                return Results.Ok(new { message = result });
            })
            .RequireAuthorization();
    }
}