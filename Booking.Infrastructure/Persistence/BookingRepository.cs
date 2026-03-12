using Booking.Application.Features.Bookings.GetHostBookings;
using Booking.Application.Features.Bookings.GetMyBookings;
using Booking.Application.Features.Bookings.Persistence;
using Booking.Domain.Entities.Bookings;
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

    public async Task<PropertyEntity?> GetPropertyWithAvailabilityAsync(
        int propertyId,
        CancellationToken cancellationToken)
    {
        return await _context.Properties
            .Include(p => p.Availabilities)
            .AsTracking()
            .FirstOrDefaultAsync(p => p.Id == propertyId, cancellationToken);
    }

    public async Task<Guid?> GetPropertyOwnerIdAsync(
        int propertyId,
        CancellationToken cancellationToken)
    {
        return await _context.Properties
            .Where(p => p.Id == propertyId)
            .Select(p => (Guid?)p.OwnerId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> AddBookingAsync(
        BookingEntity booking,
        CancellationToken cancellationToken)
    {
        await _context.Bookings.AddAsync(booking, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return booking.Id;
    }

    public async Task<BookingEntity?> GetBookingByIdAsync(
        int bookingId,
        CancellationToken cancellationToken)
    {
        return await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
    }

    public async Task BlockAvailabilityAsync(
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
            availability.IsAvailable = false;
        }
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

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(List<MyBookingDto> Items, int TotalCount)> GetGuestBookingsPagedAsync(
        Guid guestId,
        BookingStatus? status,
        string? scope,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var query =
            from booking in _context.Bookings
            join property in _context.Properties on booking.PropertyId equals property.Id
            join address in _context.Addresses on property.AddressId equals address.Id
            where booking.GuestId == guestId
            select new MyBookingDto
            {
                BookingId = booking.Id,
                PropertyId = property.Id,
                PropertyName = property.Name,
                City = address.City,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                GuestCount = booking.GuestCount,
                PriceForPeriod = booking.PriceForPeriod,
                CleaningFee = booking.CleaningFee,
                AmenitiesUpCharge = booking.AmenitiesUpCharge,
                TotalPrice = booking.PriceForPeriod + booking.CleaningFee + booking.AmenitiesUpCharge,
                BookingStatus = booking.BookingStatus.ToString(),
                IsUpcoming = booking.EndDate > today,
                CreatedAt = booking.CreatedAt
            };

        if (status.HasValue)
        {
            var statusText = status.Value.ToString();
            query = query.Where(x => x.BookingStatus == statusText);
        }

        if (!string.IsNullOrWhiteSpace(scope))
        {
            var normalizedScope = scope.Trim().ToLower();

            if (normalizedScope == "upcoming")
            {
                query = query.Where(x => x.IsUpcoming);
            }
            else if (normalizedScope == "past")
            {
                query = query.Where(x => !x.IsUpcoming);
            }
        }

        query = query.OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(List<HostBookingDto> Items, int TotalCount)> GetHostBookingsPagedAsync(
        Guid hostId,
        BookingStatus? status,
        string? scope,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var query =
            from booking in _context.Bookings
            join property in _context.Properties on booking.PropertyId equals property.Id
            where property.OwnerId == hostId
            select new HostBookingDto
            {
                BookingId = booking.Id,
                PropertyId = property.Id,
                PropertyName = property.Name,
                GuestId = booking.GuestId,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                GuestCount = booking.GuestCount,
                PriceForPeriod = booking.PriceForPeriod,
                CleaningFee = booking.CleaningFee,
                AmenitiesUpCharge = booking.AmenitiesUpCharge,
                TotalPrice = booking.PriceForPeriod + booking.CleaningFee + booking.AmenitiesUpCharge,
                BookingStatus = booking.BookingStatus.ToString(),
                IsUpcoming = booking.EndDate > today,
                CreatedAt = booking.CreatedAt
            };

        if (status.HasValue)
        {
            var statusText = status.Value.ToString();
            query = query.Where(x => x.BookingStatus == statusText);
        }

        if (!string.IsNullOrWhiteSpace(scope))
        {
            var normalizedScope = scope.Trim().ToLower();

            if (normalizedScope == "upcoming")
            {
                query = query.Where(x => x.IsUpcoming);
            }
            else if (normalizedScope == "past")
            {
                query = query.Where(x => !x.IsUpcoming);
            }
        }

        query = query.OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}