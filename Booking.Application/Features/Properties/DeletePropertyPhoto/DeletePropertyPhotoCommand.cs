using MediatR;

namespace Booking.Application.Features.Properties.DeletePropertyPhoto;

public record DeletePropertyPhotoCommand(
    Guid ActorUserId,
    int PropertyId,
    int PhotoId
) : IRequest<int>;