namespace Booking.Application.Features.Properties.UpdateProperty;

public class UpdatePropertyDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Amenities { get; set; }
    public string? Rules { get; set; }
    public string PropertyType { get; set; } = string.Empty;
    public int MaxGuests { get; set; }
    public TimeOnly CheckInTime { get; set; }
    public TimeOnly CheckOutTime { get; set; }

    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}