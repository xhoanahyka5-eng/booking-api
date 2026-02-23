using System;

namespace Booking.Domain.Entities.Properties;

public class Property
{
    public int Id { get; set; } // PK (mund të mbetet int)

    public Guid OwnerId { get; set; }        // FK -> Users.Id (GUID)
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public PropertyType PropertyType { get; set; } = PropertyType.Apartment;

    public int AddressId { get; set; }      // FK -> Addresses.Id (int)

    public int MaxGuests { get; set; }
    public TimeOnly CheckInTime { get; set; }
    public TimeOnly CheckOutTime { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsApproved { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }

    public DateTime? LastBookedOnUtc { get; set; }
}

public enum PropertyType
{
    Apartment = 0,
    House = 1,
    Room = 2,
    Villa = 3
}
