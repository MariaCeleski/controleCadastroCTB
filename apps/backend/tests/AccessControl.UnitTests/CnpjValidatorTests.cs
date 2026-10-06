using AccessControl.Application.Companies;
using Xunit;

namespace AccessControl.UnitTests;

public sealed class CnpjValidatorTests
{
    [Theory]
    [InlineData("11.222.333/0001-81")]
    [InlineData("11222333000181")]
    public void Returns_true_for_valid_cnpj(string cnpj) => Assert.True(CnpjValidator.IsValid(cnpj));

    [Theory]
    [InlineData("11.222.333/0001-80")]
    [InlineData("00.000.000/0000-00")]
    public void Returns_false_for_invalid_cnpj(string cnpj) => Assert.False(CnpjValidator.IsValid(cnpj));
}
