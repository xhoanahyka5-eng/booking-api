using Booking.Domain.Entities.OwnerProfiles;
using Booking.Domain.Entities.UserRoles;

namespace Booking.Domain.Entities.Users;

public class User
{
    public Guid Id { get; private set; }

    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;

    public string? PhoneNumber { get; private set; }
    public string? ProfileImageUrl { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public OwnerProfile? OwnerProfile { get; private set; }

    private User() { } // Required by EF

    private User(
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        string? phoneNumber)
    {
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        PhoneNumber = phoneNumber;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public static User CreateUser(
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.");

        return new User(firstName, lastName, email, passwordHash, phoneNumber);
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void ChangeEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            throw new ArgumentException("Email cannot be empty.");

        Email = newEmail;
        LastModifiedAt = DateTime.UtcNow;
    }
}