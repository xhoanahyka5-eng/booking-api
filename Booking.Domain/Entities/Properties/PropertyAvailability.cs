namespace Booking.Domain.Entities.Properties;

public class PropertyAvailability
{
    public int Id { get; set; }

    public int PropertyId { get; set; }

    public DateOnly Date { get; set; }

    public decimal Price { get; set; }

    public bool IsAvailable { get; set; }
}