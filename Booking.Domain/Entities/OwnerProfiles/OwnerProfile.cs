using System;
using Booking.Domain.Entities.Users;

namespace Booking.Domain.Entities.OwnerProfiles;

public class OwnerProfile
{
    public Guid UserId { get; set; } // PK, FK -> Users.Id (GUID)

    public string IdentityCardNumber { get; set; } = string.Empty;
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;

    public string? BusinessName { get; set; }
    public string? CreditCardToken { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }

    // Navigation
    public User? User { get; set; }
}

public enum VerificationStatus
{
    Pending = 0,
    Verified = 1,
    Rejected = 2
}
