using FluentValidation;

namespace Booking.Application.Features.Properties.SetPropertyStatus;

public class SetPropertyStatusCommandValidator
    : AbstractValidator<SetPropertyStatusCommand>
{
    public SetPropertyStatusCommandValidator()
    {
        RuleFor(x => x.ActorUserId)
            .NotEmpty().WithMessage("ActorUserId is required.");

        RuleFor(x => x.PropertyId)
            .GreaterThan(0).WithMessage("PropertyId must be greater than 0.");
    }
}