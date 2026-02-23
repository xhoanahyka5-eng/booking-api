using System;
using Booking.Domain.Entities.UserRoles;

namespace Booking.Domain.Entities.Roles;

public class Role
{
    public Guid Id { get; set; } = Guid.NewGuid(); 

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
