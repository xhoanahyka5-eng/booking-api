using Booking.Domain.Entities.Reviews;
using BookingEntity = Booking.Domain.Entities.Bookings.Booking;

namespace Booking.Application.Features.Reviews.Persistence;

public interface IReviewRepository
{
    Task<BookingEntity?> GetBookingByIdAsync(int bookingId, CancellationToken ct);

    Task<bool> HasReviewForBookingAsync(int bookingId, CancellationToken ct);

    Task AddReviewAsync(Review review, CancellationToken ct);

    Task<List<Review>> GetPropertyReviewsAsync(int propertyId, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}