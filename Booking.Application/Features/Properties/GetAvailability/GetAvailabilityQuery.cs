using MediatR;

namespace Booking.Application.Features.Properties.GetAvailability;

public record GetAvailabilityQuery(int PropertyId)
    : IRequest<List<AvailabilityDto>>;