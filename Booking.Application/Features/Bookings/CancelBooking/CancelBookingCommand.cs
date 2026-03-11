using MediatR;

namespace Booking.Application.Features.Bookings.CancelBooking;

public record CancelBookingCommand(
    int BookingId,
    Guid GuestId
) : IRequest;