using MediatR;

namespace Booking.Application.Features.Users.BecomeHost;

public record BecomeHostCommand(
    string IdentityCardNumber,
    string BusinessName
) : IRequest<string>;