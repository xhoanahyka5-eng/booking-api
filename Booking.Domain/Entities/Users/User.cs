using Booking.Domain.Entities.OwnerProfiles;
using Booking.Domain.Entities.UserRoles;

namespace Booking.Domain.Entities.Users;

public class User
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }

    // ✅ NAVIGATION PROPERTIES (mungonin)
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public OwnerProfile? OwnerProfile { get; set; }

    public static User CreateUser(
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        string? phoneNumber)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Password = passwordHash,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }
}