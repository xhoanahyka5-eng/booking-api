using Booking.Domain.Entities.Addresses;
using Booking.Domain.Entities.Properties;
using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Booking.Application.Features.Properties;

namespace Booking.Api.Features.Properties;

public static class PropertyEndpoints
{
    public static void MapPropertyEndpoints(this WebApplication app)
    {
       
        app.MapPost("/api/v1/properties",
        async (
            CreatePropertyDto dto,
            HttpContext http,
            BookingDbContext db,
            CancellationToken ct
        ) =>
        {
            var userIdStr = http.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var address = new Address
            {
                Country = dto.Country,
                City = dto.City,
                Street = dto.Street,
                PostalCode = dto.PostalCode
            };

            db.Addresses.Add(address);
            await db.SaveChangesAsync(ct);

            var property = new Property
            {
                OwnerId = userId,
                Name = dto.Name,
                Description = dto.Description,
                PropertyType = Enum.Parse<PropertyType>(dto.PropertyType),
                MaxGuests = dto.MaxGuests,
                CheckInTime = dto.CheckInTime,
                CheckOutTime = dto.CheckOutTime,
                AddressId = address.Id,
                IsActive = true,
                IsApproved = false,
                CreatedAt = DateTime.UtcNow
            };

            db.Properties.Add(property);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { property.Id });
        })
        .RequireAuthorization();



        
        app.MapPost("/api/v1/properties/{id}/availability",
        async (
            int id,
            SetAvailabilityDto dto,
            BookingDbContext db,
            CancellationToken ct
        ) =>
        {
            var propertyExists = await db.Properties
                .AnyAsync(p => p.Id == id, ct);

            if (!propertyExists)
                return Results.NotFound("Property not found");

            var exists = await db.PropertyAvailabilities
                .AnyAsync(a => a.PropertyId == id && a.Date == dto.Date, ct);

            if (exists)
                return Results.BadRequest("Availability already exists for this date");

            var availability = new PropertyAvailability
            {
                PropertyId = id,
                Date = dto.Date,
                Price = dto.Price,
                IsAvailable = dto.IsAvailable
            };

            db.PropertyAvailabilities.Add(availability);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { availability.Id });
        })
        .RequireAuthorization();




        app.MapGet("/api/v1/properties/search",
        async (
            string city,
            int guests,
            DateOnly date,
            BookingDbContext db
        ) =>
        {
            var properties = await db.Properties
                .Include(p => p.Address)
                .Where(p =>
                    p.Address.City.ToLower() == city.ToLower() &&
                    p.MaxGuests >= guests &&
                    db.PropertyAvailabilities.Any(a =>
                        a.PropertyId == p.Id &&
                        a.Date == date &&
                        a.IsAvailable))
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.MaxGuests,
                    City = p.Address.City,
                    Street = p.Address.Street
                })
                .ToListAsync();

            return Results.Ok(properties);
        });



 
        app.MapGet("/api/v1/properties",
        async (BookingDbContext db) =>
        {
            var properties = await db.Properties
                .Include(p => p.Address)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.MaxGuests,
                    City = p.Address.City,
                    Street = p.Address.Street
                })
                .ToListAsync();

            return Results.Ok(properties);
        });



   
        app.MapGet("/api/v1/properties/{id}",
        async (int id, BookingDbContext db) =>
        {
            var property = await db.Properties
                .Include(p => p.Address)
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.MaxGuests,
                    p.CheckInTime,
                    p.CheckOutTime,
                    p.PropertyType,
                    Country = p.Address.Country,
                    City = p.Address.City,
                    Street = p.Address.Street,
                    PostalCode = p.Address.PostalCode
                })
                .FirstOrDefaultAsync();

            if (property == null)
                return Results.NotFound();

            return Results.Ok(property);
        });



        app.MapGet("/api/v1/properties/{id}/availability",
        async (int id, BookingDbContext db) =>
        {
            var availability = await db.PropertyAvailabilities
                .Where(a => a.PropertyId == id)
                .Select(a => new
                {
                    a.Id,
                    a.Date,
                    a.Price,
                    a.IsAvailable
                })
                .ToListAsync();

            return Results.Ok(availability);
        });

    }
}