using Booking.Application.Features.Reviews.GetPropertyReviews;
using Booking.Application.Features.Reviews.Persistence;
using Booking.Domain.Entities.Reviews;
using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using BookingEntity = Booking.Domain.Entities.Bookings.Booking;

namespace Booking.Infrastructure.Persistence;

public class ReviewRepository : IReviewRepository
{
    private readonly BookingDbContext _context;

    public ReviewRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<int> AddReviewAsync(
        Review review,
        CancellationToken cancellationToken)
    {
        await _context.Reviews.AddAsync(review, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return review.Id;
    }

    public async Task<BookingEntity?> GetBookingByIdAsync(
        int bookingId,
        CancellationToken cancellationToken)
    {
        return await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
    }

    public async Task<bool> HasReviewForBookingAsync(
        int bookingId,
        CancellationToken cancellationToken)
    {
        return await _context.Reviews
            .AnyAsync(r => r.BookingId == bookingId, cancellationToken);
    }

    public async Task<(List<PropertyReviewDto> Items, int TotalCount, decimal AverageRating)> GetPropertyReviewsPagedAsync(
        int propertyId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query =
            from review in _context.Reviews
            join booking in _context.Bookings on review.BookingId equals booking.Id
            where booking.PropertyId == propertyId
            select new PropertyReviewDto
            {
                ReviewId = review.Id,
                BookingId = review.BookingId,
                GuestId = review.GuestId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };

        var totalCount = await query.CountAsync(cancellationToken);

        decimal averageRating = 0;

        if (totalCount > 0)
        {
            averageRating = (decimal)await _context.Reviews
                .Join(
                    _context.Bookings,
                    review => review.BookingId,
                    booking => booking.Id,
                    (review, booking) => new { review, booking })
                .Where(x => x.booking.PropertyId == propertyId)
                .AverageAsync(x => (double)x.review.Rating, cancellationToken);
        }

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount, averageRating);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}