using Booking.Domain.Entities.Users;

namespace Booking.Domain.Entities.OwnerProfiles;

public class OwnerProfile
{
    public Guid UserId { get; private set; }

    public string IdentityCardNumber { get; private set; } = null!;

    public VerificationStatus VerificationStatus { get; private set; }

    public string? BusinessName { get; private set; }

    public string? CreditCardToken { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? LastModifiedAt { get; private set; }

    public User? User { get; private set; }

    private OwnerProfile() { }

    public OwnerProfile(Guid userId, string identityCardNumber, string? businessName = null)
    {
        if (string.IsNullOrWhiteSpace(identityCardNumber))
            throw new ArgumentException("Identity card number cannot be empty.");

        UserId = userId;
        IdentityCardNumber = identityCardNumber;
        BusinessName = businessName;

        VerificationStatus = VerificationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Verify()
    {
        if (VerificationStatus == VerificationStatus.Verified)
            throw new InvalidOperationException("Owner is already verified.");

        VerificationStatus = VerificationStatus.Verified;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        if (VerificationStatus == VerificationStatus.Rejected)
            throw new InvalidOperationException("Owner is already rejected.");

        VerificationStatus = VerificationStatus.Rejected;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateBusinessName(string? businessName)
    {
        BusinessName = businessName;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateCreditCardToken(string? token)
    {
        CreditCardToken = token;
        LastModifiedAt = DateTime.UtcNow;
    }
}

public enum VerificationStatus
{
    Pending = 0,
    Verified = 1,
    Rejected = 2
}