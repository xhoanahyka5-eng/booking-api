namespace Booking.Application.Features.Properties.Users.Login;

public class LoginUserResponse
{
    public string Token { get; init; }
    public string Type { get; init; } = "Bearer";
    public int Expiration { get; init; }

    public LoginUserResponse(string token, int expiration)
    {
        Token = token;
        Expiration = expiration;
    }
}