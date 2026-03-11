namespace Booking.Application.Features.Reviews.GetPropertyReviews;

public class PropertyReviewDto
{
    public int ReviewId { get; set; }
    public int BookingId { get; set; }
    public Guid GuestId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}