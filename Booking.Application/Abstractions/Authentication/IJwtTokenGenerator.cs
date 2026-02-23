using Booking.Domain.Entities.Users;

namespace Booking.Application.Abstractions.Authentication;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}