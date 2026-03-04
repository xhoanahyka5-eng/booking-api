namespace Booking.Application.Features.Users.Login;

public class LoginUserDto
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}