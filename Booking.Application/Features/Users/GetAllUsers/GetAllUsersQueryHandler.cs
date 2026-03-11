using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Users.Persistence;
using MediatR;

namespace Booking.Application.Features.Users.GetAllUsers;

public class GetAllUsersQueryHandler
    : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserDto>> Handle(
        GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        var adminUser = await _userRepository.GetByIdWithRolesAsync(
            request.AdminId,
            cancellationToken);

        if (adminUser is null)
            throw new NotFoundException("User not found.");

        var isAdmin = adminUser.UserRoles
            .Any(ur => ur.Role != null && ur.Role.Name == "Admin");

        if (!isAdmin)
            throw new UnauthorizedException("Only admin can view all users.");

        var users = await _userRepository.GetAllAsync(cancellationToken);

        return users
            .Select(u => new UserDto(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email
            ))
            .ToList();
    }
}