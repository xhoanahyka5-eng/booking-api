using Booking.Application.Common.Models;
using Booking.Application.Features.Properties.Persistence;
using MediatR;

namespace Booking.Application.Features.Properties.GetAllProperties;

public class GetAllPropertiesQueryHandler
    : IRequestHandler<GetAllPropertiesQuery, PagedResult<PropertyDto>>
{
    private readonly IPropertyRepository _propertyRepository;

    public GetAllPropertiesQueryHandler(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<PagedResult<PropertyDto>> Handle(
        GetAllPropertiesQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize > 50 ? 50 : request.PageSize;

        var (properties, totalCount) = await _propertyRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            cancellationToken);

        var items = properties.Select(p => new PropertyDto
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

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<PropertyDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}