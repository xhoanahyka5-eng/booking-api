namespace Booking.Domain.Entities.Properties;

public class PropertyPhoto
{
    public int Id { get; set; }

    public int PropertyId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public string Base64Data { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}