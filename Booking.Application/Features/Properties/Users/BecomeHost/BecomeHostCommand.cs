using MediatR;

namespace Booking.Application.Features.Properties.Users.BecomeHost;

public record BecomeHostCommand(
    Guid UserId,
    string IdentityCardNumber,
    string BusinessName
) : IRequest<string>;