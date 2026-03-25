namespace Booking.Application.Common.Events;

public class BookingEventMessage
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public string EventType { get; set; } = default!; // booking.created, booking.confirmed, booking.rejected
    public int BookingId { get; set; }
    public int PropertyId { get; set; }
    public string GuestId { get; set; } = default!;
    public string? HostId { get; set; }
    public string Status { get; set; } = default!;
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}