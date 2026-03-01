namespace Booking.Api.Features.Users.Register;

public record RegisterUserDto(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PhoneNumber
);