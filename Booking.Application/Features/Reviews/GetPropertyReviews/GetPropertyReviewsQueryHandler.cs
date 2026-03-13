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
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize > 50 ? 50 : request.PageSize;

        var (items, totalCount, averageRating) =
            await _reviewRepository.GetPropertyReviewsPagedAsync(
                request.PropertyId,
                pageNumber,
                pageSize,
                cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PropertyReviewsResponse
        {
            Items = items,
            AverageRating = averageRating,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}