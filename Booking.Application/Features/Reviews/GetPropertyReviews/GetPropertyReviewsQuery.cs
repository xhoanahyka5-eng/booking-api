using MediatR;

namespace Booking.Application.Features.Reviews.GetPropertyReviews;

public record GetPropertyReviewsQuery(
    int PropertyId,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PropertyReviewsResponse>;