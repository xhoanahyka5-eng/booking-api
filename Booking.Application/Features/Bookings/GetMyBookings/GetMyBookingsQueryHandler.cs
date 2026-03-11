using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Bookings.Persistence;
using Booking.Domain.Entities.Bookings;
using MediatR;

namespace Booking.Application.Features.Bookings.GetMyBookings;

public class GetMyBookingsQueryHandler
    : IRequestHandler<GetMyBookingsQuery, List<MyBookingDto>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetMyBookingsQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<List<MyBookingDto>> Handle(
        GetMyBookingsQuery request,
        CancellationToken cancellationToken)
    {
        BookingStatus? parsedStatus = null;

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<BookingStatus>(request.Status, true, out var status))
                throw new ConflictException("Invalid booking status filter.");

            parsedStatus = status;
        }

        return await _bookingRepository.GetGuestBookingsAsync(
            request.GuestId,
            parsedStatus,
            request.Scope,
            cancellationToken);
    }
}