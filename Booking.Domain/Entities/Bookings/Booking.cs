using System;

namespace Booking.Domain.Entities.Bookings;

public class Booking
{
    public int Id { get; set; } // PK (mund të mbetet int)

    public int PropertyId { get; set; } // FK -> Properties.Id (int)
    public Guid GuestId { get; set; }   // FK -> Users.Id (GUID)

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public int GuestCount { get; set; }

    public decimal CleaningFee { get; set; }
    public decimal AmenitiesUpCharge { get; set; }
    public decimal PriceForPeriod { get; set; }
    public decimal TotalPrice { get; set; }

    public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }

    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedOnUtc { get; set; }
    public DateTime? RejectedOnUtc { get; set; }
    public DateTime? CompletedOnUtc { get; set; }
    public DateTime? CancelledOnUtc { get; set; }
}

public enum BookingStatus
{
    Pending = 0,
    Confirmed = 1,
    Rejected = 2,
    Completed = 3,
    Cancelled = 4
}
