using MediatR;

namespace Booking.Application.Features.Bookings.ConfirmBooking;

public record ConfirmBookingCommand(
    int BookingId,
    Guid HostId
) : IRequest;