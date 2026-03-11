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

    public async Task Handle(
        RejectBookingCommand request,
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
            throw new UnauthorizedException("You are not allowed to reject this booking.");

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