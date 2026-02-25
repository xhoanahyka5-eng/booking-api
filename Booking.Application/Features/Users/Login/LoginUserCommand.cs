using Booking.Application.Features.Users.Login;
using MediatR;

public record LoginUserCommand(
    string Email,
    string Password
) : IRequest<LoginUserResponse>;