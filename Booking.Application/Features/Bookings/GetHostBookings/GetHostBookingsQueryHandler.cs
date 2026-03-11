using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Bookings.Persistence;
using Booking.Domain.Entities.Bookings;
using MediatR;

namespace Booking.Application.Features.Bookings.GetHostBookings;

public class GetHostBookingsQueryHandler
    : IRequestHandler<GetHostBookingsQuery, List<HostBookingDto>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetHostBookingsQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<List<HostBookingDto>> Handle(
        GetHostBookingsQuery request,
        CancellationToken cancellationToken)
    {
        BookingStatus? parsedStatus = null;

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<BookingStatus>(request.Status, true, out var status))
                throw new ConflictException("Invalid booking status filter.");

            parsedStatus = status;
        }

        return await _bookingRepository.GetHostBookingsAsync(
            request.HostId,
            parsedStatus,
            request.Scope,
            cancellationToken);
    }
}