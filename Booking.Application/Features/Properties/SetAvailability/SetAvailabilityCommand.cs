using MediatR;

namespace Booking.Application.Features.Properties.SetAvailability;

public record SetAvailabilityCommand(
    Guid OwnerId,
    int PropertyId,
    DateOnly Date,
    decimal Price,
    bool IsAvailable
) : IRequest<int>;