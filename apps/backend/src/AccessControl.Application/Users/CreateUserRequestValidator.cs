using FluentValidation;

namespace AccessControl.Application.Users;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3).MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Password).MinimumLength(12).MaximumLength(128);
        RuleFor(x => x.Role).IsInEnum();
    }
}
