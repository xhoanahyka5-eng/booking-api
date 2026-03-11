using MediatR;
using Microsoft.Extensions.Logging;
using Booking.Application.Features.Users.Persistence;
using Booking.Application.Abstractions.Authentication;
using Booking.Application.Common.Exceptions;

namespace Booking.Application.Features.Users.Login;

public class LoginUserCommandHandler
    : IRequestHandler<LoginUserCommand, LoginUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<LoginUserCommandHandler> _logger;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<LoginUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<LoginUserResponse> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("User not found: {Email}", request.Email);
            throw new UnauthorizedException("Invalid credentials.");
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            _logger.LogWarning("Invalid password for: {Email}", request.Email);
            throw new UnauthorizedException("Invalid credentials.");
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        _logger.LogInformation("Login successful for: {Email}", request.Email);

        return new LoginUserResponse(token, 3600);
    }
}