namespace Booking.Application.Features.Users.Login;

public record LoginUserResponse(
    string AccessToken,
    string RefreshToken,
    int Expiration,
    string Type = "Bearer"
);