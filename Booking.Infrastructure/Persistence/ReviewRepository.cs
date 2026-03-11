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

    public async Task<BookingEntity?> GetBookingByIdAsync(int bookingId, CancellationToken ct)
    {
        return await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct);
    }

    public async Task<bool> HasReviewForBookingAsync(int bookingId, CancellationToken ct)
    {
        return await _context.Reviews
            .AnyAsync(r => r.BookingId == bookingId, ct);
    }

    public async Task AddReviewAsync(Review review, CancellationToken ct)
    {
        await _context.Reviews.AddAsync(review, ct);
    }

    public async Task<List<Review>> GetPropertyReviewsAsync(int propertyId, CancellationToken ct)
    {
        return await (
            from review in _context.Reviews
            join booking in _context.Bookings on review.BookingId equals booking.Id
            where booking.PropertyId == propertyId
            orderby review.CreatedAt descending
            select review
        ).ToListAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
}