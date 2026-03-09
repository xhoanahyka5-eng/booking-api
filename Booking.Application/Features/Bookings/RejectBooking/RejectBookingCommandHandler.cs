using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Bookings.Persistence;
using MediatR;

namespace Booking.Application.Features.Bookings.RejectBooking;

public class RejectBookingCommandHandler : IRequestHandler<RejectBookingCommand>
{
    private readonly IBookingRepository _bookingRepository;

    public RejectBookingCommandHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task Handle(RejectBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(
            request.BookingId,
            cancellationToken);

        if (booking is null)
            throw new NotFoundException("Booking not found.");

        try
        {
            booking.Reject();
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await _bookingRepository.SaveChangesAsync(cancellationToken);
    }
}