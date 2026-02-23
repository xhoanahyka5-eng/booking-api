using MediatR;
using Booking.Application.Features.Users.Persistence;
using Booking.Application.Abstractions.Authentication;
using Booking.Domain.Entities.Users;

namespace Booking.Application.Features.Users.Login;

public class LoginUserCommandHandler
    : IRequestHandler<LoginUserCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<string> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(
            request.Email,
            cancellationToken
        );

        if (user is null)
            throw new InvalidOperationException("Invalid credentials.");

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.Password
        );

        if (!isPasswordValid)
            throw new InvalidOperationException("Invalid credentials.");

        var token = _jwtTokenGenerator.GenerateToken(user);

        return token;
    }
}