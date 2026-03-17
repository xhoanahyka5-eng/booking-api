using System.Security.Cryptography;

namespace Booking.Domain.Entities.Authentication;

public static class RefreshTokenGenerator
{
    public static string Generate()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
}