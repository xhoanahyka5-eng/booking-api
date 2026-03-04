using Booking.Api.Features.Users.Login;
using Booking.Api.Features.Users.Register;
using Booking.Application.Features.Users.Login;
using Booking.Application.Features.Users.Register;
using Booking.Application.Features.Users.Persistence;
using Booking.Infrastructure.Data;
using Booking.Domain.Entities.UserRoles;
using Booking.Domain.Entities.OwnerProfiles;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

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

     
        app.MapPost("/api/v1/users/become-host",
     async (
         BecomeHostDto dto,
         HttpContext http,
         [FromServices] BookingDbContext db,
         CancellationToken ct
     ) =>
     {
         var userIdStr =
             http.User.FindFirstValue(ClaimTypes.NameIdentifier)
             ?? http.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

         if (string.IsNullOrWhiteSpace(userIdStr) ||
             !Guid.TryParse(userIdStr, out var userId))
         {
             return Results.Unauthorized();
         }

         var hostRole = await db.Roles
             .FirstAsync(r => r.Name == "Host", ct);

         var alreadyHost = await db.UserRoles
             .AnyAsync(ur =>
                 ur.UserId == userId &&
                 ur.RoleId == hostRole.Id,
                 ct);

         if (!alreadyHost)
         {
             db.UserRoles.Add(new UserRole
             {
                 UserId = userId,
                 RoleId = hostRole.Id
             });
         }

         var hasOwnerProfile = await db.OwnerProfiles
             .AnyAsync(op => op.UserId == userId, ct);

         if (!hasOwnerProfile)
         {
             var ownerProfile = new OwnerProfile(
                 userId,
                 dto.IdentityCardNumber,
                 dto.BusinessName
             );

             db.OwnerProfiles.Add(ownerProfile);
         }

         await db.SaveChangesAsync(ct);

         return Results.Ok(new { message = "You are now a Host" });
     })
 .RequireAuthorization();
    }
}