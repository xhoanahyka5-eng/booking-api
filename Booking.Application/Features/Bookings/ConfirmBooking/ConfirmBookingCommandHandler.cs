using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Bookings.Persistence;
using Booking.Application.Features.Bookings.ConfirmBooking;
using MediatR;

namespace Booking.Application.Features.Bookings.ConfirmBooking;

public class ConfirmBookingCommandHandler : IRequestHandler<ConfirmBookingCommand>
{
    private readonly IBookingRepository _bookingRepository;

    public ConfirmBookingCommandHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository
            .GetBookingByIdAsync(request.BookingId, cancellationToken);

        if (booking == null)
            throw new NotFoundException("Booking not found.");

        booking.Confirm();

        await _bookingRepository.SaveChangesAsync(cancellationToken);
    }
}