using Booking.Domain.Entities.Addresses;

namespace Booking.Domain.Entities.Properties;

public class Property
{
    public int Id { get; private set; }

    public Guid OwnerId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? Amenities { get; private set; }

    public string? Rules { get; private set; }

    public PropertyType PropertyType { get; private set; }

    public int AddressId { get; private set; }

    public Address Address { get; private set; } = null!;

    public int MaxGuests { get; private set; }

    public TimeOnly CheckInTime { get; private set; }

    public TimeOnly CheckOutTime { get; private set; }

    public bool IsActive { get; private set; }

    public bool IsApproved { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? LastModifiedAt { get; private set; }

    public ICollection<PropertyAvailability> Availabilities { get; private set; }
        = new List<PropertyAvailability>();

    public ICollection<PropertyPhoto> Photos { get; private set; }
        = new List<PropertyPhoto>();

    private Property() { }

    public Property(
        Guid ownerId,
        string name,
        string? description,
        PropertyType propertyType,
        int maxGuests,
        TimeOnly checkInTime,
        TimeOnly checkOutTime,
        int addressId)
    {
        OwnerId = ownerId;
        Name = name;
        Description = description;
        PropertyType = propertyType;
        MaxGuests = maxGuests;
        CheckInTime = checkInTime;
        CheckOutTime = checkOutTime;
        AddressId = addressId;

        IsActive = true;
        IsApproved = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddAvailability(DateOnly date, decimal price, bool isAvailable)
    {
        Availabilities.Add(new PropertyAvailability
        {
            PropertyId = Id,
            Date = date,
            Price = price,
            IsAvailable = isAvailable
        });

        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(
        string name,
        string? description,
        int maxGuests,
        TimeOnly checkInTime,
        TimeOnly checkOutTime)
    {
        Name = name;
        Description = description;
        MaxGuests = maxGuests;
        CheckInTime = checkInTime;
        CheckOutTime = checkOutTime;

        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateAmenitiesAndRules(string? amenities, string? rules)
    {
        Amenities = amenities;
        Rules = rules;

        LastModifiedAt = DateTime.UtcNow;
    }

    public void ChangePropertyType(PropertyType propertyType)
    {
        PropertyType = propertyType;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void ChangeAddress(int addressId)
    {
        AddressId = addressId;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        IsApproved = true;
        LastModifiedAt = DateTime.UtcNow;
    }
}

public enum PropertyType
{
    Apartment = 0,
    House = 1,
    Room = 2,
    Villa = 3
}