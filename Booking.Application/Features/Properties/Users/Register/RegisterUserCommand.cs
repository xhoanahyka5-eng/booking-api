using MediatR;
using Booking.Domain.Entities.Users;

namespace Booking.Application.Features.Properties.Users.Register;

public record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PhoneNumber
) : IRequest<RegisterUserResponse>;