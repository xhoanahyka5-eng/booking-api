namespace Booking.Application.Features.Properties;

public class PropertyDto
{
    public int Id { get; set; }
    public Guid OwnerId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Amenities { get; set; }
    public string? Rules { get; set; }

    public string PropertyType { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;

    public int MaxGuests { get; set; }
    public TimeOnly CheckInTime { get; set; }
    public TimeOnly CheckOutTime { get; set; }

    public bool IsActive { get; set; }
    public bool IsApproved { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}