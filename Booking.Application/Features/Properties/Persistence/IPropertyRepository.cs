using Booking.Domain.Entities.Addresses;
using Booking.Domain.Entities.Properties;

namespace Booking.Application.Features.Properties.Persistence;

public interface IPropertyRepository
{
    Task<bool> PropertyExists(int propertyId, CancellationToken ct);

    Task<bool> AvailabilityExists(int propertyId, DateOnly date, CancellationToken ct);

    Task<int> AddAvailability(PropertyAvailability availability, CancellationToken ct);

    Task<List<PropertyAvailability>> GetAvailabilityByPropertyId(int propertyId, CancellationToken ct);

    Task<int> CreateProperty(Property property, CancellationToken ct);

    Task<Address> GetOrCreateAddressAsync(
        string country,
        string city,
        string street,
        string postalCode,
        CancellationToken ct);

    Task<Property?> GetPropertyById(int propertyId, CancellationToken ct);

    Task<Property?> GetPropertyWithAvailabilityAsync(int propertyId, CancellationToken ct);

    Task<Property?> GetPropertyWithPhotosAsync(int propertyId, CancellationToken ct);

    Task AddPhotoAsync(PropertyPhoto photo, CancellationToken ct);

    Task RemovePhotoAsync(PropertyPhoto photo, CancellationToken ct);

    Task<List<PropertyPhoto>> GetPhotosByPropertyIdAsync(int propertyId, CancellationToken ct);

    Task UpsertAvailabilityAsync(
        int propertyId,
        DateOnly date,
        decimal price,
        bool isAvailable,
        CancellationToken ct);

    Task SaveChanges(CancellationToken ct);

    Task<List<Property>> GetAllAsync(CancellationToken ct);

    Task<(List<Property> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken ct);

    Task<List<Property>> SearchAsync(
        string city,
        int guests,
        DateOnly date,
        string? propertyType,
        decimal? minPrice,
        decimal? maxPrice,
        CancellationToken ct);

    Task<(List<Property> Items, int TotalCount)> SearchPagedAsync(
        string city,
        int guests,
        DateOnly date,
        string? propertyType,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber,
        int pageSize,
        CancellationToken ct);
}