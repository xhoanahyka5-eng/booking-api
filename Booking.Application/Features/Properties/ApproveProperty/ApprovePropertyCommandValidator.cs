using FluentValidation;

namespace Booking.Application.Features.Properties.ApproveProperty;

public class ApprovePropertyCommandValidator
    : AbstractValidator<ApprovePropertyCommand>
{
    public ApprovePropertyCommandValidator()
    {
        RuleFor(x => x.AdminId)
            .NotEmpty().WithMessage("AdminId is required.");

        RuleFor(x => x.PropertyId)
            .GreaterThan(0).WithMessage("PropertyId must be greater than 0.");
    }
}