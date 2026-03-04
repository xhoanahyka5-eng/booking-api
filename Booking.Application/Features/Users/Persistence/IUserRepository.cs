using Booking.Domain.Entities.Users;
using Booking.Application.Abstractions.Persistence;

namespace Booking.Application.Features.Users.Persistence;

public interface IUserRepository : IGenericRepository<User>
{
    Task<bool> IsEmailUnique(string email, CancellationToken cancellationToken);
    Task<Guid> GetRoleIdByNameAsync(string roleName, CancellationToken cancellationToken);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
}