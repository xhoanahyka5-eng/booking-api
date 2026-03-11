using FluentValidation;

namespace Booking.Application.Features.Properties.AddPropertyPhoto;

public class AddPropertyPhotoCommandValidator
    : AbstractValidator<AddPropertyPhotoCommand>
{
    public AddPropertyPhotoCommandValidator()
    {
        RuleFor(x => x.ActorUserId)
            .NotEmpty();

        RuleFor(x => x.PropertyId)
            .GreaterThan(0);

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(x => x.StartsWith("image/"))
            .WithMessage("ContentType must be an image type.");

        RuleFor(x => x.Base64Data)
            .NotEmpty();
    }
}