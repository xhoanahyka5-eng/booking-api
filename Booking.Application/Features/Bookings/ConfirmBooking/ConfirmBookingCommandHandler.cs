using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Bookings.Persistence;
using MediatR;

namespace Booking.Application.Features.Bookings.ConfirmBooking;

public class ConfirmBookingCommandHandler : IRequestHandler<ConfirmBookingCommand>
{
    private readonly IBookingRepository _bookingRepository;

    public ConfirmBookingCommandHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task Handle(
        ConfirmBookingCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(
            request.BookingId,
            cancellationToken);

        if (booking is null)
            throw new NotFoundException("Booking not found.");

        var ownerId = await _bookingRepository.GetPropertyOwnerIdAsync(
            booking.PropertyId,
            cancellationToken);

        if (ownerId is null)
            throw new NotFoundException("Property not found.");

        if (ownerId.Value != request.HostId)
            throw new UnauthorizedException("You are not allowed to confirm this booking.");

        try
        {
            booking.Confirm();
        }
        catch (Exception ex)
        {
            throw new ConflictException(ex.Message);
        }

        await _bookingRepository.BlockAvailabilityAsync(
            booking.PropertyId,
            booking.StartDate,
            booking.EndDate,
            cancellationToken);

        await _bookingRepository.SaveChangesAsync(cancellationToken);
    }
}