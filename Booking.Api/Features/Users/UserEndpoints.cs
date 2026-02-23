using Booking.Application.Features.Users.Register;
using Booking.Application.Features.Users.Login;
using MediatR;

namespace Booking.Api.Features.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/users/register",
            async (CreateUserDto dto, ISender sender, CancellationToken ct) =>
            {
                var command = new RegisterUserCommand(dto);
                var id = await sender.Send(command, ct);
                return Results.Ok(id);
            });

        app.MapPost("/api/v1/users/login",
            async (LoginUserCommand command, ISender sender, CancellationToken ct) =>
            {
                var token = await sender.Send(command, ct);
                return Results.Ok(new { token });
            });
    }
}