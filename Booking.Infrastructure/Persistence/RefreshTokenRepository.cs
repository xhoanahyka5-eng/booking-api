using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Booking.Application.Features.Users.Persistence;
using Booking.Domain.Entities.Authentication;
using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure.Persistence;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly BookingDbContext _db;

    public RefreshTokenRepository(BookingDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(RefreshToken token, CancellationToken ct)
    {
        await _db.RefreshTokens.AddAsync(token, ct);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct)
    {
        return await _db.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == token, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _db.SaveChangesAsync(ct);
    }

    public async Task RevokeAllActiveForUserAsync(Guid userId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var tokens = await _db.RefreshTokens
            .Where(t =>
                t.UserId == userId &&
                t.RevokedAtUtc == null &&
                t.ExpiresAtUtc > now)
            .ToListAsync(ct);

        foreach (var token in tokens)
            token.Revoke();

        if (tokens.Count > 0)
            await _db.SaveChangesAsync(ct);
    }

    public async Task RevokeByTokenForUserAsync(string refreshToken, Guid userId, CancellationToken ct)
    {
        var stored = await GetByTokenAsync(refreshToken, ct);
        if (stored is null || stored.UserId != userId)
            return;

        if (stored.IsActive)
            stored.Revoke();

        await _db.SaveChangesAsync(ct);
    }
}