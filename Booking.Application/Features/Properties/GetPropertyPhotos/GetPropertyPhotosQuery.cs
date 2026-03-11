using MediatR;

namespace Booking.Application.Features.Properties.GetPropertyPhotos;

public record GetPropertyPhotosQuery(int PropertyId)
    : IRequest<List<PropertyPhotoDto>>;