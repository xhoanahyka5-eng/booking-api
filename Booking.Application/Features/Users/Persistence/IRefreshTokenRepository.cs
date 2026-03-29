using Booking.Domain.Entities.Authentication;

namespace Booking.Application.Features.Users.Persistence;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken ct);

    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);

    Task RevokeAllActiveForUserAsync(Guid userId, CancellationToken ct);

    Task RevokeByTokenForUserAsync(string refreshToken, Guid userId, CancellationToken ct);
}