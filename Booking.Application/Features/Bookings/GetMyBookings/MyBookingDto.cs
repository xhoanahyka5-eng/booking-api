namespace Booking.Application.Features.Bookings.GetMyBookings;

public class MyBookingDto
{
    public int BookingId { get; set; }
    public int PropertyId { get; set; }

    public string PropertyName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public int GuestCount { get; set; }

    public decimal PriceForPeriod { get; set; }
    public decimal CleaningFee { get; set; }
    public decimal AmenitiesUpCharge { get; set; }
    public decimal TotalPrice { get; set; }

    public string BookingStatus { get; set; } = string.Empty;

    public bool IsUpcoming { get; set; }

    public DateTime CreatedAt { get; set; }
}