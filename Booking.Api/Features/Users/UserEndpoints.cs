using Booking.Application.Features.Users.Login;
using Booking.Application.Features.Users.Persistence;
using Booking.Application.Features.Users.Register;
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

        app.MapGet("/api/v1/users", async (IUserRepository repo) =>
        {
            var users = await repo.GetAllAsync(CancellationToken.None);
            return Results.Ok(users);
        });
    }
}