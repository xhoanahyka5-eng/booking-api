using Booking.Application.Features.Properties.Persistence;
using MediatR;

namespace Booking.Application.Features.Properties.GetAllProperties;

public class GetAllPropertiesQueryHandler
    : IRequestHandler<GetAllPropertiesQuery, List<PropertyDto>>
{
    private readonly IPropertyRepository _propertyRepository;

    public GetAllPropertiesQueryHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<List<PropertyDto>> Handle(
        GetAllPropertiesQuery request,
        CancellationToken cancellationToken)
    {
        var properties = await _propertyRepository.GetAllAsync(cancellationToken);

        return properties.Select(p => new PropertyDto
        {
            Id = p.Id,
            OwnerId = p.OwnerId,
            Name = p.Name,
            Description = p.Description,
            Amenities = p.Amenities,
            Rules = p.Rules,
            PropertyType = p.PropertyType.ToString(),
            Country = p.Address.Country,
            City = p.Address.City,
            Street = p.Address.Street,
            PostalCode = p.Address.PostalCode,
            MaxGuests = p.MaxGuests,
            CheckInTime = p.CheckInTime,
            CheckOutTime = p.CheckOutTime,
            IsActive = p.IsActive,
            IsApproved = p.IsApproved,
            CreatedAt = p.CreatedAt,
            LastModifiedAt = p.LastModifiedAt
        }).ToList();
    }
}