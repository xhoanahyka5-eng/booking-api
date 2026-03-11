using Booking.Application.Abstractions.Persistence;
using Booking.Domain.Entities.OwnerProfiles;
using Booking.Domain.Entities.UserRoles;
using Booking.Domain.Entities.Users;

namespace Booking.Application.Features.Users.Persistence;

public interface IUserRepository : IGenericRepository<User>
{
    Task<bool> IsEmailUnique(string email, CancellationToken cancellationToken);
    Task<Guid> GetRoleIdByNameAsync(string roleName, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> GetByIdWithRolesAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> HasOwnerProfileAsync(Guid userId, CancellationToken cancellationToken);
    Task AddOwnerProfileAsync(OwnerProfile ownerProfile, CancellationToken cancellationToken);
    Task AddUserRoleAsync(UserRole userRole, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}