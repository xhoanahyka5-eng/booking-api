using Booking.Application.Features.Properties.Users.Login;
using MediatR;

public record LoginUserCommand(
    string Email,
    string Password
) : IRequest<LoginUserResponse>;