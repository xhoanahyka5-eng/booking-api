using MediatR;

namespace Booking.Application.Features.Properties.AddPropertyPhoto;

public record AddPropertyPhotoCommand(
    Guid ActorUserId,
    int PropertyId,
    string FileName,
    string ContentType,
    string Base64Data,
    bool IsPrimary
) : IRequest<int>;