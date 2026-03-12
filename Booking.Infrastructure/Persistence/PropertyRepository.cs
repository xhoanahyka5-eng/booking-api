using Booking.Application.Features.Properties.Persistence;
using Booking.Domain.Entities.Addresses;
using Booking.Domain.Entities.Properties;
using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure.Persistence;

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

    public async Task<Address> GetOrCreateAddressAsync(
        string country,
        string city,
        string street,
        string postalCode,
        CancellationToken ct)
    {
        var existingAddress = await _db.Addresses
            .FirstOrDefaultAsync(a =>
                a.Country == country &&
                a.City == city &&
                a.Street == street &&
                a.PostalCode == postalCode,
                ct);

        if (existingAddress is not null)
            return existingAddress;

        var newAddress = new Address
        {
            Country = country,
            City = city,
            Street = street,
            PostalCode = postalCode
        };

        _db.Addresses.Add(newAddress);

        try
        {
            await _db.SaveChangesAsync(ct);
            return newAddress;
        }
        catch (DbUpdateException)
        {
            _db.Entry(newAddress).State = EntityState.Detached;

            existingAddress = await _db.Addresses
                .FirstOrDefaultAsync(a =>
                    a.Country == country &&
                    a.City == city &&
                    a.Street == street &&
                    a.PostalCode == postalCode,
                    ct);

            if (existingAddress is not null)
                return existingAddress;

            throw;
        }
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
            .OrderBy(a => a.Date)
            .ToListAsync(ct);
    }

    public async Task<List<Property>> GetAllAsync(CancellationToken ct)
    {
        return await _db.Properties
            .Include(p => p.Address)
            .ToListAsync(ct);
    }

    public async Task<(List<Property> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken ct)
    {
        var query = _db.Properties
            .AsNoTracking()
            .Include(p => p.Address)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<Property?> GetPropertyById(int propertyId, CancellationToken ct)
    {
        return await _db.Properties
            .Include(p => p.Address)
            .Include(p => p.Availabilities)
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.Id == propertyId, ct);
    }

    public async Task<Property?> GetPropertyWithAvailabilityAsync(
        int propertyId,
        CancellationToken ct)
    {
        return await _db.Properties
            .Include(p => p.Availabilities)
            .FirstOrDefaultAsync(p => p.Id == propertyId, ct);
    }

    public async Task<Property?> GetPropertyWithPhotosAsync(
        int propertyId,
        CancellationToken ct)
    {
        return await _db.Properties
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.Id == propertyId, ct);
    }

    public async Task AddPhotoAsync(PropertyPhoto photo, CancellationToken ct)
    {
        await _db.PropertyPhotos.AddAsync(photo, ct);
    }

    public async Task<List<PropertyPhoto>> GetPhotosByPropertyIdAsync(
        int propertyId,
        CancellationToken ct)
    {
        return await _db.PropertyPhotos
            .Where(p => p.PropertyId == propertyId)
            .OrderByDescending(p => p.IsPrimary)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task UpsertAvailabilityAsync(
        int propertyId,
        DateOnly date,
        decimal price,
        bool isAvailable,
        CancellationToken ct)
    {
        var existingAvailability = await _db.PropertyAvailabilities
            .FirstOrDefaultAsync(
                a => a.PropertyId == propertyId && a.Date == date,
                ct);

        if (existingAvailability is null)
        {
            _db.PropertyAvailabilities.Add(new PropertyAvailability
            {
                PropertyId = propertyId,
                Date = date,
                Price = price,
                IsAvailable = isAvailable
            });

            return;
        }

        existingAvailability.Price = price;
        existingAvailability.IsAvailable = isAvailable;
    }

    public async Task<List<Property>> SearchAsync(
        string city,
        int guests,
        DateOnly date,
        string? propertyType,
        decimal? minPrice,
        decimal? maxPrice,
        CancellationToken ct)
    {
        var query = _db.Properties
            .Include(p => p.Address)
            .Include(p => p.Availabilities)
            .Where(p =>
                p.IsActive &&
                p.IsApproved &&
                p.Address.City == city &&
                p.MaxGuests >= guests &&
                p.Availabilities.Any(a => a.Date == date && a.IsAvailable));

        if (!string.IsNullOrWhiteSpace(propertyType) &&
            Enum.TryParse<PropertyType>(propertyType, true, out var parsedType))
        {
            query = query.Where(p => p.PropertyType == parsedType);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p =>
                p.Availabilities.Any(a =>
                    a.Date == date &&
                    a.IsAvailable &&
                    a.Price >= minPrice.Value));
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p =>
                p.Availabilities.Any(a =>
                    a.Date == date &&
                    a.IsAvailable &&
                    a.Price <= maxPrice.Value));
        }

        return await query.ToListAsync(ct);
    }

    public async Task<(List<Property> Items, int TotalCount)> SearchPagedAsync(
        string city,
        int guests,
        DateOnly date,
        string? propertyType,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber,
        int pageSize,
        CancellationToken ct)
    {
        var query = _db.Properties
            .AsNoTracking()
            .Include(p => p.Address)
            .Include(p => p.Availabilities)
            .Where(p =>
                p.IsActive &&
                p.IsApproved &&
                p.Address.City == city &&
                p.MaxGuests >= guests &&
                p.Availabilities.Any(a => a.Date == date && a.IsAvailable));

        if (!string.IsNullOrWhiteSpace(propertyType) &&
            Enum.TryParse<PropertyType>(propertyType, true, out var parsedType))
        {
            query = query.Where(p => p.PropertyType == parsedType);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p =>
                p.Availabilities.Any(a =>
                    a.Date == date &&
                    a.IsAvailable &&
                    a.Price >= minPrice.Value));
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p =>
                p.Availabilities.Any(a =>
                    a.Date == date &&
                    a.IsAvailable &&
                    a.Price <= maxPrice.Value));
        }

        query = query.OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task SaveChanges(CancellationToken ct)
    {
        await _db.SaveChangesAsync(ct);
    }
}