using Booking.Domain.Entities.Properties;

namespace Booking.Api.Features.Properties;

public record CreatePropertyDto(
    string Name,
    string? Description,
    PropertyType PropertyType,
    int MaxGuests,
    TimeOnly CheckInTime,
    TimeOnly CheckOutTime,
    string Country,
    string City,
    string Street,
    string PostalCode
);