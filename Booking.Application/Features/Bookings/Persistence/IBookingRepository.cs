using Booking.Application.Features.Bookings.GetHostBookings;
using Booking.Application.Features.Bookings.GetMyBookings;
using Booking.Domain.Entities.Bookings;

using BookingEntity = Booking.Domain.Entities.Bookings.Booking;
using PropertyEntity = Booking.Domain.Entities.Properties.Property;

namespace Booking.Application.Features.Bookings.Persistence;

public interface IBookingRepository
{
    Task<int> AddBookingAsync(
        BookingEntity booking,
        CancellationToken cancellationToken);

    Task<BookingEntity?> GetBookingByIdAsync(
        int bookingId,
        CancellationToken cancellationToken);

    Task<PropertyEntity?> GetPropertyWithAvailabilityAsync(
        int propertyId,
        CancellationToken cancellationToken);

    Task<Guid?> GetPropertyOwnerIdAsync(
        int propertyId,
        CancellationToken cancellationToken);

    Task BlockAvailabilityAsync(
        int propertyId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken);

    Task RestoreAvailabilityAsync(
        int propertyId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken);

    Task<(List<MyBookingDto> Items, int TotalCount)> GetGuestBookingsPagedAsync(
        Guid guestId,
        BookingStatus? status,
        string? scope,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<(List<HostBookingDto> Items, int TotalCount)> GetHostBookingsPagedAsync(
        Guid hostId,
        BookingStatus? status,
        string? scope,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}