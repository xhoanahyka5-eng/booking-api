using FluentValidation;

namespace Booking.Application.Features.Properties.DeletePropertyPhoto;

public class DeletePropertyPhotoCommandValidator
    : AbstractValidator<DeletePropertyPhotoCommand>
{
    public DeletePropertyPhotoCommandValidator()
    {
        RuleFor(x => x.ActorUserId)
            .NotEmpty();

        RuleFor(x => x.PropertyId)
            .GreaterThan(0);

        RuleFor(x => x.PhotoId)
            .GreaterThan(0);
    }
}