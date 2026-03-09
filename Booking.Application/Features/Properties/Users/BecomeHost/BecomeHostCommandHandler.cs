using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Properties.Users.Persistence;
using Booking.Domain.Entities.OwnerProfiles;
using Booking.Domain.Entities.UserRoles;
using MediatR;

namespace Booking.Application.Features.Properties.Users.BecomeHost;

public class BecomeHostCommandHandler : IRequestHandler<BecomeHostCommand, string>
{
    private readonly IUserRepository _repo;

    public BecomeHostCommandHandler(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<string> Handle(BecomeHostCommand request, CancellationToken ct)
    {
        var user = await _repo.GetByIdWithRolesAsync(request.UserId, ct);

        if (user is null)
            throw new NotFoundException("User not found.");

        var alreadyHasOwnerProfile = await _repo.HasOwnerProfileAsync(request.UserId, ct);

        if (alreadyHasOwnerProfile)
            throw new ConflictException("User is already a host.");

        var alreadyHost = user.UserRoles.Any(ur => ur.Role != null && ur.Role.Name == "Host");

        if (alreadyHost)
            throw new ConflictException("User already has Host role.");

        var ownerProfile = new OwnerProfile(
            request.UserId,
            request.IdentityCardNumber,
            request.BusinessName
        );

        var hostRoleId = await _repo.GetRoleIdByNameAsync("Host", ct);

        var userRole = new UserRole
        {
            UserId = request.UserId,
            RoleId = hostRoleId,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddOwnerProfileAsync(ownerProfile, ct);
        await _repo.AddUserRoleAsync(userRole, ct);
        await _repo.SaveChangesAsync(ct);

        return "You are now a Host";
    }
}