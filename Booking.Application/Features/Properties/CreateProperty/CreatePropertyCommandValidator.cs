using FluentValidation;

namespace Booking.Application.Features.Properties.CreateProperty;

public class CreatePropertyCommandValidator
    : AbstractValidator<CreatePropertyCommand>
{
    public CreatePropertyCommandValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("OwnerId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.PropertyType)
            .NotEmpty().WithMessage("Property type is required.");

        RuleFor(x => x.MaxGuests)
            .GreaterThan(0).WithMessage("MaxGuests must be greater than 0.");

        RuleFor(x => x.CheckInTime)
            .NotEmpty().WithMessage("Check-in time is required.");

        RuleFor(x => x.CheckOutTime)
            .NotEmpty().WithMessage("Check-out time is required.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters.");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required.")
            .MaximumLength(200).WithMessage("Street cannot exceed 200 characters.");

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Postal code is required.")
            .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters.");
    }
}