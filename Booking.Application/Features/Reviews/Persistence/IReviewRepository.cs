using Booking.Application.Features.Reviews.GetPropertyReviews;
using Booking.Domain.Entities.Reviews;

using BookingEntity = Booking.Domain.Entities.Bookings.Booking;

namespace Booking.Application.Features.Reviews.Persistence;

public interface IReviewRepository
{
    Task<int> AddReviewAsync(Review review, CancellationToken cancellationToken);

    Task<BookingEntity?> GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken);

    Task<bool> HasReviewForBookingAsync(int bookingId, CancellationToken cancellationToken);

    Task<(List<PropertyReviewDto> Items, int TotalCount, decimal AverageRating)> GetPropertyReviewsPagedAsync(
        int propertyId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}