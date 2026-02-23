using Booking.Application.Abstractions.Persistence;
using Booking.Application.Features.Users.Persistence;
using Booking.Domain.Entities.Users;
using Booking.Infrastructure.Data;
using Booking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure.Users;

public class UserRepository
    : GenericRepository<User>, IUserRepository
{
    public UserRepository(BookingDbContext context)
        : base(context)
    {
    }

    public async Task<bool> IsEmailUnique(string email, CancellationToken cancellationToken)
    {
        return !await _context.Users
            .AnyAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }
}