namespace AccessControl.Application.Companies;

public sealed record CredentialRequest(string ModuleKey, string Label, string Username, string Password);
public sealed record CreateCompanyRequest(string CompanyName, string Cnpj, string? StateRegistration, IReadOnlyCollection<CredentialRequest> Credentials);
public sealed record CompanySummary(Guid Id, string CompanyName, string Cnpj, string? StateRegistration, DateTimeOffset UpdatedAt);

public interface ICompanyService
{
    Task<CompanySummary> CreateAsync(Guid organizationId, CreateCompanyRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CompanySummary>> SearchAsync(Guid organizationId, string? search, CancellationToken cancellationToken);
}
