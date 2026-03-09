using BookingEntity = Booking.Domain.Entities.Bookings.Booking;
using PropertyEntity = Booking.Domain.Entities.Properties.Property;

namespace Booking.Application.Features.Bookings.Persistence;

public interface IBookingRepository
{
    Task<PropertyEntity?> GetPropertyWithAvailabilityAsync(
        int propertyId,
        CancellationToken cancellationToken);

    Task<BookingEntity?> GetBookingByIdAsync(
        int bookingId,
        CancellationToken cancellationToken);

    Task<int> AddBookingAsync(
        BookingEntity booking,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);

    Task RestoreAvailabilityAsync(
    int propertyId,
    DateOnly startDate,
    DateOnly endDate,
    CancellationToken cancellationToken);
}