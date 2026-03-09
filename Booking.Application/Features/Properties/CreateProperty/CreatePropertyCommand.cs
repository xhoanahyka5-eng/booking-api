using MediatR;

namespace Booking.Application.Features.Properties.CreateProperty;

public record CreatePropertyCommand(
    Guid OwnerId,
    string Name,
    string Description,
    string PropertyType,
    int MaxGuests,
    TimeOnly CheckInTime,
    TimeOnly CheckOutTime,
    string Country,
    string City,
    string Street,
    string PostalCode
) : IRequest<int>;