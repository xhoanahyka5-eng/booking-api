using Booking.Application.Common.Exceptions;
using Booking.Application.Common.Models;
using Booking.Application.Features.Bookings.Persistence;
using Booking.Domain.Entities.Bookings;
using MediatR;

namespace Booking.Application.Features.Bookings.GetHostBookings;

public class GetHostBookingsQueryHandler
    : IRequestHandler<GetHostBookingsQuery, PagedResult<HostBookingDto>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetHostBookingsQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<PagedResult<HostBookingDto>> Handle(
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

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize > 50 ? 50 : request.PageSize;

        var (items, totalCount) = await _bookingRepository.GetHostBookingsPagedAsync(
            request.HostId,
            parsedStatus,
            request.Scope,
            pageNumber,
            pageSize,
            cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<HostBookingDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}