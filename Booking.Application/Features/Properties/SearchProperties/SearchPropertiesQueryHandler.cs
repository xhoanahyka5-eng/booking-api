using Booking.Application.Features.Properties.Persistence;
using MediatR;

namespace Booking.Application.Features.Properties.SearchProperties;

public class SearchPropertiesQueryHandler
    : IRequestHandler<SearchPropertiesQuery, List<PropertyDto>>
{
    private readonly IPropertyRepository _propertyRepository;

    public SearchPropertiesQueryHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<List<PropertyDto>> Handle(
        SearchPropertiesQuery request,
        CancellationToken cancellationToken)
    {
        var properties = await _propertyRepository.SearchAsync(
            request.City,
            request.Guests,
            request.Date,
            request.PropertyType,
            request.MinPrice,
            request.MaxPrice,
            cancellationToken);

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