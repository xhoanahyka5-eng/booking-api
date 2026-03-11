using MediatR;

namespace Booking.Application.Features.Bookings.GetMyBookings;

public record GetMyBookingsQuery(
    Guid GuestId,
    string? Status,
    string? Scope
) : IRequest<List<MyBookingDto>>;