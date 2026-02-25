using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using Booking.Application.Features.Users.Persistence;
using Booking.Application.Abstractions.Authentication;

namespace Booking.Application.Features.Users.Login;

public class LoginUserCommandHandler
    : IRequestHandler<LoginUserCommand, LoginUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<LoginUserCommandHandler> _logger;
    private readonly IValidator<LoginUserCommand> _validator;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<LoginUserCommandHandler> logger,
        IValidator<LoginUserCommand> validator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
        _validator = validator;
    }

    public async Task<LoginUserResponse> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Login validation failed for {Email}", request.Email);
            throw new ValidationException(validationResult.Errors);
        }

        _logger.LogInformation("Login attempt for email: {Email}", request.Email);

        var user = await _userRepository.GetByEmailAsync(
            request.Email,
            cancellationToken
        );

        if (user is null)
        {
            _logger.LogWarning("Login failed. User not found: {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash
        );

        if (!isPasswordValid)
        {
            _logger.LogWarning("Login failed. Invalid password for: {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        _logger.LogInformation("Login successful for: {Email}", request.Email);

        return new LoginUserResponse(token, 3600);
    }
}