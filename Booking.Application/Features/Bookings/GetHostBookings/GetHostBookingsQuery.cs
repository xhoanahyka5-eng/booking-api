using MediatR;

namespace Booking.Application.Features.Bookings.GetHostBookings;

public record GetHostBookingsQuery(
    Guid HostId,
    string? Status,
    string? Scope
) : IRequest<List<HostBookingDto>>;