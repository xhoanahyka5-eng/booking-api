using MediatR;
using Booking.Domain.Entities.Users;   // ✅ KJO ËSHTË E SAKTË

namespace Booking.Application.Features.Users.Register;

public record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PhoneNumber
) : IRequest<RegisterUserResponse>;