using FluentValidation;

namespace Booking.Application.Features.Properties.SearchProperties;

public class SearchPropertiesQueryValidator : AbstractValidator<SearchPropertiesQuery>
{
    public SearchPropertiesQueryValidator()
    {
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters.");

        RuleFor(x => x.Guests)
            .GreaterThan(0).WithMessage("Guests must be greater than 0.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.");
    }
}