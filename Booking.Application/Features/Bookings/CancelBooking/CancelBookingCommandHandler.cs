using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Bookings.Persistence;
using Booking.Domain.Entities.Bookings;
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

        if (booking.GuestId != request.GuestId)
            throw new UnauthorizedException("You are not allowed to cancel this booking.");

        var wasConfirmed = booking.BookingStatus == BookingStatus.Confirmed;

        try
        {
            booking.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        if (wasConfirmed)
        {
            await _bookingRepository.RestoreAvailabilityAsync(
                booking.PropertyId,
                booking.StartDate,
                booking.EndDate,
                cancellationToken);
        }

        await _bookingRepository.SaveChangesAsync(cancellationToken);
    }
}