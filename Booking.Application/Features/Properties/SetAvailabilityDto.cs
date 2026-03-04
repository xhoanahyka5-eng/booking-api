namespace Booking.Application.Features.Properties;

public class SetAvailabilityDto
{
    public DateOnly Date { get; set; }

    public decimal Price { get; set; }

    public bool IsAvailable { get; set; }
}