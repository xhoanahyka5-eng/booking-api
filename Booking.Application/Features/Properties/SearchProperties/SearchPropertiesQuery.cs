using Booking.Application.Features.Properties.GetAllProperties;
using MediatR;

namespace Booking.Application.Features.Properties.SearchProperties;

public record SearchPropertiesQuery(
    string City,
    int Guests,
    DateOnly Date
) : IRequest<List<PropertyDto>>;