namespace Booking.Application.Features.Bookings.CreateBooking;

public class CreateBookingDto
{
    public int PropertyId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int GuestCount { get; set; }
}