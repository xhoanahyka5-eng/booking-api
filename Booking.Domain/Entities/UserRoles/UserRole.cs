using System;
using Booking.Domain.Entities.Roles;
using Booking.Domain.Entities.Users;

namespace Booking.Domain.Entities.UserRoles;

public class UserRole
{
    public Guid UserId { get; set; }     // FK -> Users.Id (Guid) ✅
    public Guid RoleId { get; set; }     // FK -> Roles.Id (Guid) ✅

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User? User { get; set; }
    public Role? Role { get; set; }
}
