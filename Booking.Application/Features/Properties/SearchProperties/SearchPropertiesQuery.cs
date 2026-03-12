using Booking.Application.Common.Models;
using MediatR;

namespace Booking.Application.Features.Properties.SearchProperties;

public record SearchPropertiesQuery(
    string City,
    int Guests,
    DateOnly Date,
    string? PropertyType,
    decimal? MinPrice,
    decimal? MaxPrice,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResult<PropertyDto>>;