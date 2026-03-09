using Booking.Application.Features.Properties.Users.BecomeHost;
using Booking.Application.Features.Properties.Users.GetAllUsers;
using Booking.Application.Features.Properties.Users.Login;
using Booking.Application.Features.Properties.Users.Register;
using MediatR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Booking.Api.Features.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/users/register",
            async (
                RegisterUserDto dto,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var command = new RegisterUserCommand(
                    dto.FirstName,
                    dto.LastName,
                    dto.Email,
                    dto.Password,
                    dto.PhoneNumber
                );

                var result = await sender.Send(command, ct);

                return Results.Ok(result);
            });

        app.MapPost("/api/v1/users/login",
            async (
                LoginUserDto dto,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var command = new LoginUserCommand(
                    dto.Email,
                    dto.Password
                );

                var result = await sender.Send(command, ct);

                return Results.Ok(result);
            });

        app.MapGet("/api/v1/users",
            async (
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var query = new GetAllUsersQuery();

                var result = await sender.Send(query, ct);

                return Results.Ok(result);
            })
        .RequireAuthorization();

        app.MapPost("/api/v1/users/become-host",
            async (
                BecomeHostDto dto,
                HttpContext http,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var userIdStr =
                    http.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? http.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

                if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                    return Results.Unauthorized();

                var command = new BecomeHostCommand(
                    userId,
                    dto.IdentityCardNumber,
                    dto.BusinessName
                );

                var result = await sender.Send(command, ct);

                return Results.Ok(new { message = result });
            })
        .RequireAuthorization();
    }
}