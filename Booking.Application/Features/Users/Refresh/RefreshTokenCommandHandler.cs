using Booking.Application.Abstractions.Authentication;
using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Users.Login;
using Booking.Application.Features.Users.Persistence;
using Booking.Domain.Entities.Authentication;
using MediatR;

namespace Booking.Application.Features.Users.Refresh;

public class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, LoginUserResponse>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginUserResponse> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(
            request.RefreshToken,
            cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
            throw new UnauthorizedException("Invalid refresh token.");

        var user = storedToken.User!;

        var newAccessToken = _jwtTokenGenerator.GenerateToken(user);

        var newRefreshTokenValue = RefreshTokenGenerator.Generate();

        storedToken.Revoke(newRefreshTokenValue);

        var newRefreshToken = new RefreshToken(
            user.Id,
            newRefreshTokenValue,
            DateTime.UtcNow.AddDays(7)
        );

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new LoginUserResponse(
            newAccessToken,
            newRefreshTokenValue,
            3600
        );
    }
}