using Booking.Application.Features.Users.Login;
using MediatR;

namespace Booking.Application.Features.Users.Refresh;

public record RefreshTokenCommand(string RefreshToken)
    : IRequest<LoginUserResponse>;