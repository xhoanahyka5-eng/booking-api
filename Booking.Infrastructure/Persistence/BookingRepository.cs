using Booking.Application.Features.Bookings.Persistence;
using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BookingEntity = Booking.Domain.Entities.Bookings.Booking;
using PropertyEntity = Booking.Domain.Entities.Properties.Property;

namespace Booking.Infrastructure.Persistence;

public class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _context;

    public BookingRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<PropertyEntity?> GetPropertyWithAvailabilityAsync(int propertyId, CancellationToken cancellationToken)
    {
        return await _context.Properties
            .Include(p => p.Availabilities)
            .FirstOrDefaultAsync(p => p.Id == propertyId, cancellationToken);
    }

    public async Task<int> AddBookingAsync(BookingEntity booking, CancellationToken cancellationToken)
    {
        await _context.Bookings.AddAsync(booking, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return booking.Id;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<BookingEntity?> GetBookingByIdAsync(
    int bookingId,
    CancellationToken cancellationToken)
    {
        return await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
    }

    public async Task RestoreAvailabilityAsync(
    int propertyId,
    DateOnly startDate,
    DateOnly endDate,
    CancellationToken cancellationToken)
    {
        var availabilities = await _context.PropertyAvailabilities
            .Where(a =>
                a.PropertyId == propertyId &&
                a.Date >= startDate &&
                a.Date < endDate)
            .ToListAsync(cancellationToken);

        foreach (var availability in availabilities)
        {
            availability.IsAvailable = true;
        }
    }
}