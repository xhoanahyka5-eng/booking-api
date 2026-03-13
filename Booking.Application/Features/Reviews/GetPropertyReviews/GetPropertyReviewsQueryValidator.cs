using FluentValidation;

namespace Booking.Application.Features.Reviews.GetPropertyReviews;

public class GetPropertyReviewsQueryValidator
    : AbstractValidator<GetPropertyReviewsQuery>
{
    public GetPropertyReviewsQueryValidator()
    {
        RuleFor(x => x.PropertyId)
            .GreaterThan(0);

        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50);
    }
}