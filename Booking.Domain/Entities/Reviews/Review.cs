using System;

namespace Booking.Domain.Entities.Reviews;

public class Review
{
    public int Id { get; set; } // PK

    public int BookingId { get; set; } // FK -> Bookings.Id (int)
    public Guid GuestId { get; set; }  // FK -> Users.Id (GUID)

    public int Rating { get; set; }
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
