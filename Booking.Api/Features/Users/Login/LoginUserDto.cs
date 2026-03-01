namespace Booking.Api.Features.Users.Login;

public record LoginUserDto(
    string Email,
    string Password
);