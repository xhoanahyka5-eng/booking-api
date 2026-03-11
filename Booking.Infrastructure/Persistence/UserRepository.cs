using Booking.Application.Features.Users.Persistence;
using Booking.Domain.Entities.OwnerProfiles;
using Booking.Domain.Entities.UserRoles;
using Booking.Domain.Entities.Users;
using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure.Persistence;

public class UserRepository : GenericRepository<User>, IUserRepository
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
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<User?> GetByIdWithRolesAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.OwnerProfile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<bool> HasOwnerProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Set<OwnerProfile>()
            .AnyAsync(op => op.UserId == userId, cancellationToken);
    }

    public async Task AddOwnerProfileAsync(OwnerProfile ownerProfile, CancellationToken cancellationToken)
    {
        await _context.Set<OwnerProfile>().AddAsync(ownerProfile, cancellationToken);
    }

    public async Task AddUserRoleAsync(UserRole userRole, CancellationToken cancellationToken)
    {
        await _context.Set<UserRole>().AddAsync(userRole, cancellationToken);
    }

    public async Task<Guid> GetRoleIdByNameAsync(string roleName, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);

        if (role is null)
            throw new Exception($"Role '{roleName}' not found. Seeder should have created it.");

        return role.Id;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}