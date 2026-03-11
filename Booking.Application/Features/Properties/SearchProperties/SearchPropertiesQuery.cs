using Booking.Application.Features.Properties;
using MediatR;

namespace Booking.Application.Features.Properties.SearchProperties;

public record SearchPropertiesQuery(
    string City,
    int Guests,
    DateOnly Date,
    string? PropertyType,
    decimal? MinPrice,
    decimal? MaxPrice
) : IRequest<List<PropertyDto>>;