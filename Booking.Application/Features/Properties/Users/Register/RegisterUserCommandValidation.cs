using FluentValidation;

namespace Booking.Application.Features.Properties.Users.Register;

public class RegisterUserCommandValidation
    : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidation()
    {
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(20);
    }
}