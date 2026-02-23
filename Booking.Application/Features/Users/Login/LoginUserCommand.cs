using MediatR;

namespace Booking.Application.Features.Users.Login;

public record LoginUserCommand(
    string Email,
    string Password
) : IRequest<string>;