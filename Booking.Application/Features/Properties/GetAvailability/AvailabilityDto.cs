namespace Booking.Application.Features.Properties.GetAvailability;

public class AvailabilityDto
{
    public DateOnly Date { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
}