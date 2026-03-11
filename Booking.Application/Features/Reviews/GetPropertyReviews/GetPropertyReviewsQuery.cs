using MediatR;

namespace Booking.Application.Features.Reviews.GetPropertyReviews;

public record GetPropertyReviewsQuery(int PropertyId)
    : IRequest<PropertyReviewsResponse>;