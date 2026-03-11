using Booking.Application.Features.Properties.Persistence;
using MediatR;

namespace Booking.Application.Features.Properties.GetPropertyPhotos;

public class GetPropertyPhotosQueryHandler
    : IRequestHandler<GetPropertyPhotosQuery, List<PropertyPhotoDto>>
{
    private readonly IPropertyRepository _propertyRepository;

    public GetPropertyPhotosQueryHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<List<PropertyPhotoDto>> Handle(
        GetPropertyPhotosQuery request,
        CancellationToken cancellationToken)
    {
        var photos = await _propertyRepository.GetPhotosByPropertyIdAsync(
            request.PropertyId,
            cancellationToken);

        return photos.Select(p => new PropertyPhotoDto
        {
            Id = p.Id,
            FileName = p.FileName,
            ContentType = p.ContentType,
            Base64Data = p.Base64Data,
            IsPrimary = p.IsPrimary,
            CreatedAt = p.CreatedAt
        }).ToList();
    }
}