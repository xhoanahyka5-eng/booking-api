namespace Booking.Application.Features.Properties.CreateProperty;

public class CreatePropertyDto
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string PropertyType { get; set; } = default!;
    public int MaxGuests { get; set; }
    public TimeOnly CheckInTime { get; set; }
    public TimeOnly CheckOutTime { get; set; }
    public string Country { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Street { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
}