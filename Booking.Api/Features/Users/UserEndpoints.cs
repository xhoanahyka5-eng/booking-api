using Booking.Api.Features.Users.Login;
using Booking.Api.Features.Users.Register;
using Booking.Application.Features.Users.Login;
using Booking.Application.Features.Users.Register;
using Booking.Application.Features.Users.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Api.Features.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {

        app.MapPost("/api/v1/users/register",
            async (
                RegisterUserDto dto,
                [FromServices] ISender sender,
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
                [FromServices] ISender sender,
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
            async ([FromServices] IUserRepository repo) =>
            {
                var users = await repo.GetAllAsync(CancellationToken.None);
                return Results.Ok(users);
            })
        .RequireAuthorization();
    }
}