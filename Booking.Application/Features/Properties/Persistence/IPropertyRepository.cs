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

    Task<Property?> GetPropertyById(int propertyId, CancellationToken ct);

    Task SaveChanges(CancellationToken ct);

    Task<List<Property>> GetAllAsync(CancellationToken ct);

    Task AddAddress(Address address, CancellationToken ct);

    Task<List<Property>> SearchAsync(string city, int guests, DateOnly date, CancellationToken ct);
}