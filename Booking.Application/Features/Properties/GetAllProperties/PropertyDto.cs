namespace Booking.Application.Features.Properties;

public class PropertyDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string City { get; set; } = string.Empty;

    public int MaxGuests { get; set; }
}