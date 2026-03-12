using FluentValidation;

namespace Booking.Application.Features.Properties.GetAllProperties;

public class GetAllPropertiesQueryValidator
    : AbstractValidator<GetAllPropertiesQuery>
{
    public GetAllPropertiesQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50);
    }
}