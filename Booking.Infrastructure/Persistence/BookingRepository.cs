using Booking.Application.Features.Bookings.GetHostBookings;
using Booking.Application.Features.Bookings.GetMyBookings;
using Booking.Application.Features.Bookings.Persistence;
using Booking.Domain.Entities.Bookings;
using Booking.Domain.Entities.Properties;
using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
// ✅ FIX për konflikt namespace
using BookingEntity = Booking.Domain.Entities.Bookings.Booking;
using PropertyEntity = Booking.Domain.Entities.Properties.Property;

namespace Booking.Infrastructure.Persistence;

public class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _db;

    public BookingRepository(BookingDbContext db)
    {
        _db = db;
    }

    public async Task<int> AddBookingAsync(
        BookingEntity booking,
        CancellationToken ct)
    {
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync(ct);
        return booking.Id;
    }

    public async Task<BookingEntity?> GetBookingByIdAsync(
        int bookingId,
        CancellationToken ct)
    {
        return await _db.Bookings
            .Include(b => b.Property)
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct);
    }

    public async Task<PropertyEntity?> GetPropertyWithAvailabilityAsync(
        int propertyId,
        CancellationToken ct)
    {
        return await _db.Properties
            .Include(p => p.Availabilities)
            .FirstOrDefaultAsync(p => p.Id == propertyId, ct);
    }

    public async Task<Guid?> GetPropertyOwnerIdAsync(
        int propertyId,
        CancellationToken ct)
    {
        return await _db.Properties
            .Where(p => p.Id == propertyId)
            .Select(p => p.OwnerId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task BlockAvailabilityAsync(
        int propertyId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken ct)
    {
        var dates = Enumerable.Range(0, endDate.DayNumber - startDate.DayNumber)
            .Select(offset => startDate.AddDays(offset));

        var availabilities = await _db.PropertyAvailabilities
            .Where(a => a.PropertyId == propertyId && dates.Contains(a.Date))
            .ToListAsync(ct);

        foreach (var a in availabilities)
        {
            a.IsAvailable = false;
        }
    }

    public async Task RestoreAvailabilityAsync(
        int propertyId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken ct)
    {
        var dates = Enumerable.Range(0, endDate.DayNumber - startDate.DayNumber)
            .Select(offset => startDate.AddDays(offset));

        var availabilities = await _db.PropertyAvailabilities
            .Where(a => a.PropertyId == propertyId && dates.Contains(a.Date))
            .ToListAsync(ct);

        foreach (var a in availabilities)
        {
            a.IsAvailable = true;
        }
    }

    public async Task<(List<MyBookingDto> Items, int TotalCount)> GetGuestBookingsPagedAsync(
        Guid guestId,
        BookingStatus? status,
        string? scope,
        int pageNumber,
        int pageSize,
        CancellationToken ct)
    {
        var query = _db.Bookings
            .Include(b => b.Property)
            .ThenInclude(p => p.Address)
            .Where(b => b.GuestId == guestId);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new MyBookingDto
            {
                BookingId = b.Id,
                PropertyId = b.PropertyId,

                PropertyName = b.Property.Name,
                City = b.Property.Address.City,

                StartDate = b.StartDate,
                EndDate = b.EndDate,

                GuestCount = b.GuestCount,

                PriceForPeriod = b.PriceForPeriod,
                CleaningFee = b.CleaningFee,
                AmenitiesUpCharge = b.AmenitiesUpCharge,
                TotalPrice = b.TotalPrice,

                BookingStatus = b.BookingStatus.ToString(),

                IsUpcoming = b.StartDate > DateOnly.FromDateTime(DateTime.UtcNow),

                CreatedAt = b.CreatedAt
            })
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<(List<HostBookingDto> Items, int TotalCount)> GetHostBookingsPagedAsync(
        Guid hostId,
        BookingStatus? status,
        string? scope,
        int pageNumber,
        int pageSize,
        CancellationToken ct)
    {
        var query = _db.Bookings
            .Include(b => b.Property)
            .ThenInclude(p => p.Address)
            .Where(b => b.Property.OwnerId == hostId);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new HostBookingDto
            {
                BookingId = b.Id,
                PropertyId = b.PropertyId,

                PropertyName = b.Property.Name,

                GuestId = b.GuestId,

                StartDate = b.StartDate,
                EndDate = b.EndDate,

                GuestCount = b.GuestCount,

                PriceForPeriod = b.PriceForPeriod,
                CleaningFee = b.CleaningFee,
                AmenitiesUpCharge = b.AmenitiesUpCharge,
                TotalPrice = b.TotalPrice,

                BookingStatus = b.BookingStatus.ToString(),

                IsUpcoming = b.StartDate > DateOnly.FromDateTime(DateTime.UtcNow),

                CreatedAt = b.CreatedAt
            })
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<List<BookingEntity>> GetConfirmedBookingsToCompleteAsync(
        DateOnly today,
        CancellationToken ct)
    {
        return await _db.Bookings
            .Where(b => b.BookingStatus == BookingStatus.Confirmed && b.EndDate < today)
            .ToListAsync(ct);
    }

    public async Task<List<BookingEntity>> GetPendingBookingsToExpireAsync(
        DateTime cutoffUtc,
        CancellationToken ct)
    {
        return await _db.Bookings
            .Where(b => b.BookingStatus == BookingStatus.Pending && b.CreatedAt < cutoffUtc)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(
        int propertyId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken ct)
    {
        return await _db.Bookings
            .AnyAsync(b =>
                b.PropertyId == propertyId &&
                b.StartDate < endDate &&
                b.EndDate > startDate,
                ct);
    }

    public async Task MarkUnavailableAsync(
        int propertyId,
        DateOnly date,
        CancellationToken ct)
    {
        var availability = await _db.PropertyAvailabilities
            .FirstOrDefaultAsync(a =>
                a.PropertyId == propertyId &&
                a.Date == date,
                ct);

        if (availability is not null)
        {
            availability.IsAvailable = false;
        }
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _db.SaveChangesAsync(ct);
    }
}