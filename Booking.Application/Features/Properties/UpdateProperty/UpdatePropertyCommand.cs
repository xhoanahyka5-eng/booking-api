using MediatR;

namespace Booking.Application.Features.Properties.UpdateProperty;

public record UpdatePropertyCommand(
    Guid OwnerId,
    int PropertyId,
    string Name,
    string? Description,
    string? Amenities,
    string? Rules,
    string PropertyType,
    int MaxGuests,
    TimeOnly CheckInTime,
    TimeOnly CheckOutTime,
    string Country,
    string City,
    string Street,
    string PostalCode
) : IRequest<int>;