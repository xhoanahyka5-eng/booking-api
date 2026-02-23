using FluentValidation;

namespace Booking.Application.Features.Users.Register;

public class RegisterUserCommandValidation
    : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidation()
    {
        RuleFor(x => x.UserDto.Password)
            .NotEmpty()
            .MinimumLength(6);

        RuleFor(x => x.UserDto.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.UserDto.FirstName)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.UserDto.LastName)
            .NotEmpty()
            .MaximumLength(30);
    }
}
