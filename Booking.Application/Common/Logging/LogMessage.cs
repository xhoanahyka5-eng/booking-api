namespace Booking.Application.Common.Logging;

public class LogMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Service { get; set; } = "Booking.Api";
    public string Level { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? Exception { get; set; }
    public string? UserId { get; set; }
    public string? TraceId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}