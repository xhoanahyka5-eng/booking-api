using Booking.Application.Common.Models;
using MediatR;

namespace Booking.Application.Features.Bookings.GetMyBookings;

public record GetMyBookingsQuery(
    Guid GuestId,
    string? Status,
    string? Scope,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResult<MyBookingDto>>;