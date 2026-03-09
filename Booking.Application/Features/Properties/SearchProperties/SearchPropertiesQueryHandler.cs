using Booking.Application.Features.Properties.GetAllProperties;
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
            cancellationToken);

        return properties.Select(p => new PropertyDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            City = p.Address.City,
            MaxGuests = p.MaxGuests
        }).ToList();
    }
}