using FluentValidation;

namespace AccessControl.Application.Security;

public sealed class RegisterOrganizationRequestValidator : AbstractValidator<RegisterOrganizationRequest>
{
    public RegisterOrganizationRequestValidator()
    {
        RuleFor(x => x.OrganizationName).NotEmpty().MinimumLength(3).MaximumLength(150);
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3).MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Password).MinimumLength(12).MaximumLength(128);
    }
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
