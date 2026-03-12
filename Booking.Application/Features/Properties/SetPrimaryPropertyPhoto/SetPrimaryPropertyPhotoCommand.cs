using MediatR;

namespace Booking.Application.Features.Properties.SetPrimaryPropertyPhoto;

public record SetPrimaryPropertyPhotoCommand(
    Guid ActorUserId,
    int PropertyId,
    int PhotoId
) : IRequest<int>;