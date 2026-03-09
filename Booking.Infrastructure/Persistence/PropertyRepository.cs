using Booking.Application.Features.Properties.Persistence;
using Booking.Domain.Entities.Properties;
using Booking.Domain.Entities.Addresses;
using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly BookingDbContext _db;

    public PropertyRepository(BookingDbContext db)
    {
        _db = db;
    }

    public async Task<int> CreateProperty(Property property, CancellationToken ct)
    {
        _db.Properties.Add(property);
        await _db.SaveChangesAsync(ct);

        return property.Id;
    }

    public async Task AddAddress(Address address, CancellationToken ct)
    {
        _db.Addresses.Add(address);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> PropertyExists(int propertyId, CancellationToken ct)
    {
        return await _db.Properties
            .AnyAsync(p => p.Id == propertyId, ct);
    }

    public async Task<bool> AvailabilityExists(
        int propertyId,
        DateOnly date,
        CancellationToken ct)
    {
        return await _db.PropertyAvailabilities
            .AnyAsync(a => a.PropertyId == propertyId && a.Date == date, ct);
    }

    public async Task<int> AddAvailability(
        PropertyAvailability availability,
        CancellationToken ct)
    {
        _db.PropertyAvailabilities.Add(availability);
        await _db.SaveChangesAsync(ct);

        return availability.Id;
    }

    public async Task<List<PropertyAvailability>> GetAvailabilityByPropertyId(
        int propertyId,
        CancellationToken ct)
    {
        return await _db.PropertyAvailabilities
            .Where(a => a.PropertyId == propertyId)
            .ToListAsync(ct);
    }

    public async Task<List<Property>> GetAllAsync(CancellationToken ct)
    {
        return await _db.Properties
            .Include(p => p.Address)
            .ToListAsync(ct);
    }

    public async Task<Property?> GetPropertyById(int propertyId, CancellationToken ct)
    {
        return await _db.Properties
            .Include(p => p.Address)
            .Include(p => p.Availabilities)
            .FirstOrDefaultAsync(p => p.Id == propertyId, ct);
    }

    public async Task<List<Property>> SearchAsync(
        string city,
        int guests,
        DateOnly date,
        CancellationToken ct)
    {
        return await _db.Properties
            .Include(p => p.Address)
            .Include(p => p.Availabilities)
            .Where(p =>
                p.Address.City == city &&
                p.MaxGuests >= guests &&
                p.Availabilities.Any(a => a.Date == date && a.IsAvailable))
            .ToListAsync(ct);
    }

    public async Task SaveChanges(CancellationToken ct)
    {
        await _db.SaveChangesAsync(ct);
    }
}