namespace Booking.Api.Features.Users;

public record BecomeHostDto(
    string IdentityCardNumber,
    string? BusinessName
);