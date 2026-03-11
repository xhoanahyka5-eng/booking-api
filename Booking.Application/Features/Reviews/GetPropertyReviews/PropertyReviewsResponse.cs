namespace Booking.Application.Features.Reviews.GetPropertyReviews;

public class PropertyReviewsResponse
{
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public List<PropertyReviewDto> Reviews { get; set; } = new();
}