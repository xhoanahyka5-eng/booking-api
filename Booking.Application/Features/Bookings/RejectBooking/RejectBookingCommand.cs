using MediatR;

namespace Booking.Application.Features.Bookings.RejectBooking;

public record RejectBookingCommand(
    int BookingId
) : IRequest;