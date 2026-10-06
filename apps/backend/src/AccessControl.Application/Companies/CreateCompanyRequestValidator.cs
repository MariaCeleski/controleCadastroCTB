using FluentValidation;

namespace AccessControl.Application.Companies;

public sealed class CreateCompanyRequestValidator : AbstractValidator<CreateCompanyRequest>
{
    public CreateCompanyRequestValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().MinimumLength(3).MaximumLength(200);
        RuleFor(x => x.Cnpj).Must(CnpjValidator.IsValid).WithMessage("CNPJ inválido.");
        RuleFor(x => x.Credentials).NotNull().Must(x => x.Count == 4).WithMessage("Exatamente quatro módulos são obrigatórios.");
        RuleForEach(x => x.Credentials).ChildRules(credential =>
        {
            credential.RuleFor(x => x.ModuleKey).NotEmpty().MaximumLength(50);
            credential.RuleFor(x => x.Label).NotEmpty().MaximumLength(100);
            credential.RuleFor(x => x.Username).MaximumLength(200);
            credential.RuleFor(x => x.Password).MaximumLength(500);
        });
    }
}

public static class CnpjValidator
{
    public static bool IsValid(string? value)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length != 14 || digits.Distinct().Count() == 1) return false;
        return CalculateDigit(digits[..12]) == digits[12] - '0' && CalculateDigit(digits[..13]) == digits[13] - '0';
    }

    private static int CalculateDigit(string digits)
    {
        var factor = digits.Length - 7;
        var sum = 0;
        foreach (var digit in digits)
        {
            sum += (digit - '0') * factor;
            factor = factor == 2 ? 9 : factor - 1;
        }
        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}
