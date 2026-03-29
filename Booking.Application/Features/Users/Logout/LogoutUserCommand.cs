using MediatR;

namespace Booking.Application.Features.Users.Logout;

public record LogoutUserCommand(
    Guid UserId,
    string? RefreshToken
) : IRequest<Unit>;
