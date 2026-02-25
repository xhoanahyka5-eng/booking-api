using System;
using Booking.Domain.Entities.Roles;
using Booking.Domain.Entities.Users;

namespace Booking.Domain.Entities.UserRoles;

public class UserRole
{
    public Guid UserId { get; set; }     
    public Guid RoleId { get; set; }    

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    
    public User? User { get; set; }
    public Role? Role { get; set; }
}
