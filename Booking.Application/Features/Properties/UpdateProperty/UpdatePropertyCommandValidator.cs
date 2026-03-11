using Booking.Domain.Entities.Properties;
using FluentValidation;

namespace Booking.Application.Features.Properties.UpdateProperty;

public class UpdatePropertyCommandValidator
    : AbstractValidator<UpdatePropertyCommand>
{
    public UpdatePropertyCommandValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty();

        RuleFor(x => x.PropertyId)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Amenities)
            .MaximumLength(2000);

        RuleFor(x => x.Rules)
            .MaximumLength(2000);

        RuleFor(x => x.PropertyType)
            .NotEmpty()
            .Must(x => Enum.TryParse<PropertyType>(x, true, out _))
            .WithMessage("Invalid property type.");

        RuleFor(x => x.MaxGuests)
            .GreaterThan(0);

        RuleFor(x => x.Country)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Street)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .MaximumLength(20);
    }
}