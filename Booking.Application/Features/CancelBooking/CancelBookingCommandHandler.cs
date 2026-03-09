using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Bookings.Persistence;
using MediatR;

namespace Booking.Application.Features.Bookings.CancelBooking;

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand>
{
    private readonly IBookingRepository _bookingRepository;

    public CancelBookingCommandHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task Handle(
        CancelBookingCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(
            request.BookingId,
            cancellationToken);

        if (booking is null)
            throw new NotFoundException("Booking not found.");

        booking.Cancel();

        await _bookingRepository.RestoreAvailabilityAsync(
            booking.PropertyId,
            booking.StartDate,
            booking.EndDate,
            cancellationToken);

        await _bookingRepository.SaveChangesAsync(cancellationToken);
    }
}