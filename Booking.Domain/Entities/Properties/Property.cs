using Booking.Domain.Entities.Addresses;

namespace Booking.Domain.Entities.Properties;

public class Property
{
    public int Id { get; set; }

    public Guid OwnerId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public PropertyType PropertyType { get; set; }

    public int AddressId { get; set; }

    public Address Address { get; set; } = null!;

    public int MaxGuests { get; set; }

    public TimeOnly CheckInTime { get; set; }

    public TimeOnly CheckOutTime { get; set; }

    public bool IsActive { get; set; }

    public bool IsApproved { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastModifiedAt { get; set; }
}

public enum PropertyType
{
    Apartment = 0,
    House = 1,
    Room = 2,
    Villa = 3
}