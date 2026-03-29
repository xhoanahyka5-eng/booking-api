using Booking.Application.Features.Users.Persistence;
using MediatR;

namespace Booking.Application.Features.Users.Logout;

public class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand, Unit>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LogoutUserCommandHandler(IRefreshTokenRepository refreshTokenRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<Unit> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            await _refreshTokenRepository.RevokeByTokenForUserAsync(
                request.RefreshToken,
                request.UserId,
                cancellationToken);
        }
        else
        {
            await _refreshTokenRepository.RevokeAllActiveForUserAsync(
                request.UserId,
                cancellationToken);
        }

        return Unit.Value;
    }
}
