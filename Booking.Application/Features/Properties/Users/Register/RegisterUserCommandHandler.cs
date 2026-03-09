using MediatR;
using Booking.Domain.Entities.Users;
using Booking.Domain.Entities.UserRoles;
using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Properties.Users.Persistence;

namespace Booking.Application.Features.Properties.Users.Register;

public class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IUserRepository _userRepository;

    public RegisterUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<RegisterUserResponse> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var isUnique = await _userRepository
            .IsEmailUnique(request.Email, cancellationToken);

        if (!isUnique)
            throw new ConflictException("Email already exists.");

        var passwordHash = BCrypt.Net.BCrypt
            .HashPassword(request.Password);

        var user = User.CreateUser(
            request.FirstName,
            request.LastName,
            request.Email,
            passwordHash,
            request.PhoneNumber
        );

        var guestRoleId = await _userRepository
            .GetRoleIdByNameAsync("Guest", cancellationToken);

        user.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = guestRoleId
        });

        await _userRepository.AddAsync(user, cancellationToken);

        return new RegisterUserResponse(user.Id);
    }
}