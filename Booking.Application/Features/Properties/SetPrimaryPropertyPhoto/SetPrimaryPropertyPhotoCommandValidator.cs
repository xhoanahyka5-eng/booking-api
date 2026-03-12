using FluentValidation;

namespace Booking.Application.Features.Properties.SetPrimaryPropertyPhoto;

public class SetPrimaryPropertyPhotoCommandValidator
    : AbstractValidator<SetPrimaryPropertyPhotoCommand>
{
    public SetPrimaryPropertyPhotoCommandValidator()
    {
        RuleFor(x => x.ActorUserId)
            .NotEmpty();

        RuleFor(x => x.PropertyId)
            .GreaterThan(0);

        RuleFor(x => x.PhotoId)
            .GreaterThan(0);
    }
}