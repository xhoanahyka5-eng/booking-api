using MediatR;

namespace Booking.Application.Features.Properties.SetPropertyStatus;

public record SetPropertyStatusCommand(
    Guid ActorUserId,
    int PropertyId,
    bool IsActive
) : IRequest<int>;