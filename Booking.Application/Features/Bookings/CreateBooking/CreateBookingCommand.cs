using MediatR;

namespace Booking.Application.Features.Bookings.CreateBooking;

public record CreateBookingCommand(
    Guid GuestId,
    int PropertyId,
    DateOnly StartDate,
    DateOnly EndDate,
    int GuestCount
) : IRequest<int>;