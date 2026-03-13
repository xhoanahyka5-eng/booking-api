namespace Booking.Application.Features.Reviews.GetPropertyReviews;

public class PropertyReviewsResponse
{
    public List<PropertyReviewDto> Items { get; set; } = new();

    public decimal AverageRating { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;
}