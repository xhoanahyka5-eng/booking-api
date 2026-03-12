using Booking.Application.Common.Models;
using MediatR;

namespace Booking.Application.Features.Bookings.GetHostBookings;

public record GetHostBookingsQuery(
    Guid HostId,
    string? Status,
    string? Scope,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResult<HostBookingDto>>;