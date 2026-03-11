using Booking.Application.Features.Reviews.Persistence;
using MediatR;

namespace Booking.Application.Features.Reviews.GetPropertyReviews;

public class GetPropertyReviewsQueryHandler
    : IRequestHandler<GetPropertyReviewsQuery, PropertyReviewsResponse>
{
    private readonly IReviewRepository _reviewRepository;

    public GetPropertyReviewsQueryHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<PropertyReviewsResponse> Handle(
        GetPropertyReviewsQuery request,
        CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetPropertyReviewsAsync(
            request.PropertyId,
            cancellationToken);

        var reviewDtos = reviews
            .Select(r => new PropertyReviewDto
            {
                ReviewId = r.Id,
                BookingId = r.BookingId,
                GuestId = r.GuestId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToList();

        var averageRating = reviewDtos.Count == 0
            ? 0
            : Math.Round((decimal)reviewDtos.Average(r => r.Rating), 2);

        return new PropertyReviewsResponse
        {
            AverageRating = averageRating,
            ReviewCount = reviewDtos.Count,
            Reviews = reviewDtos
        };
    }
}