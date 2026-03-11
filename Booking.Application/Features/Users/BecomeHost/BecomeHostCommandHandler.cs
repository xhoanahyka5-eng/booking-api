using Booking.Application.Abstractions.Authentication;
using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Users.Persistence;
using Booking.Domain.Entities.OwnerProfiles;
using Booking.Domain.Entities.UserRoles;
using MediatR;

namespace Booking.Application.Features.Users.BecomeHost;

public class BecomeHostCommandHandler
    : IRequestHandler<BecomeHostCommand, string>
{
    private readonly IUserRepository _repo;
    private readonly ICurrentUserService _currentUserService;

    public BecomeHostCommandHandler(
        IUserRepository repo,
        ICurrentUserService currentUserService)
    {
        _repo = repo;
        _currentUserService = currentUserService;
    }

    public async Task<string> Handle(
        BecomeHostCommand request,
        CancellationToken ct)
    {
        var userId = _currentUserService.UserId;

        var user = await _repo.GetByIdWithRolesAsync(userId, ct);

        if (user is null)
            throw new NotFoundException("User not found.");

        var alreadyHostRole = user.UserRoles
            .Any(ur => ur.Role != null && ur.Role.Name == "Host");

        var hasOwnerProfile = await _repo.HasOwnerProfileAsync(userId, ct);

        if (alreadyHostRole || hasOwnerProfile)
            throw new ConflictException("User is already a host.");

        var ownerProfile = new OwnerProfile(
            userId,
            request.IdentityCardNumber,
            request.BusinessName
        );

        await _repo.AddOwnerProfileAsync(ownerProfile, ct);

        var hostRoleId = await _repo.GetRoleIdByNameAsync("Host", ct);

        await _repo.AddUserRoleAsync(new UserRole
        {
            UserId = userId,
            RoleId = hostRoleId
        }, ct);

        await _repo.SaveChangesAsync(ct);

        return "You are now a Host";
    }
}