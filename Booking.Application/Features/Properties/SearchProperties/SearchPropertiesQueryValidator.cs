using FluentValidation;

namespace Booking.Application.Features.Properties.SearchProperties;

public class SearchPropertiesQueryValidator
    : AbstractValidator<SearchPropertiesQuery>
{
    public SearchPropertiesQueryValidator()
    {
        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Guests)
            .GreaterThan(0);

        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50);

        RuleFor(x => x.SortBy)
            .Must(x =>
                string.IsNullOrWhiteSpace(x) ||
                x.Equals("priceAsc", StringComparison.OrdinalIgnoreCase) ||
                x.Equals("priceDesc", StringComparison.OrdinalIgnoreCase) ||
                x.Equals("newest", StringComparison.OrdinalIgnoreCase) ||
                x.Equals("ratingAsc", StringComparison.OrdinalIgnoreCase) ||
                x.Equals("ratingDesc", StringComparison.OrdinalIgnoreCase) ||
                x.Equals("popularity", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortBy must be priceAsc, priceDesc, newest, ratingAsc, ratingDesc, or popularity.");

        RuleFor(x => x.MinRating)
            .InclusiveBetween(1, 5)
            .When(x => x.MinRating.HasValue);

        RuleFor(x => x)
            .Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice <= x.MaxPrice)
            .WithMessage("MinPrice cannot be greater than MaxPrice.");
    }
}