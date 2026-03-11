namespace Booking.Application.Features.Users.GetAllUsers;

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email
);