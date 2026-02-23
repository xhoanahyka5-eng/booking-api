using MediatR;
using Booking.Application.Features.Users.Persistence;
using Booking.Domain.Entities.Users;

namespace Booking.Application.Features.Users.Register;

public class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;

    public RegisterUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Guid> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var isUnique = await _userRepository.IsEmailUnique(
            request.UserDto.Email,
            cancellationToken
        );

        if (!isUnique)
            throw new InvalidOperationException("Email already exists.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(
            request.UserDto.Password
        );

        var user = User.CreateUser(
            request.UserDto.FirstName,
            request.UserDto.LastName,
            request.UserDto.Email,
            passwordHash,
            request.UserDto.PhoneNumber
        );

        await _userRepository.AddAsync(user, cancellationToken);

        return user.Id;
    }
}