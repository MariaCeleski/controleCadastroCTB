namespace AccessControl.Application.Companies;

public sealed record CredentialRequest(string ModuleKey, string Label, string Username, string Password);
public sealed record CreateCompanyRequest(string CompanyName, string Cnpj, string? StateRegistration, IReadOnlyCollection<CredentialRequest> Credentials);
public sealed record CompanySummary(Guid Id, string CompanyName, string Cnpj, string? StateRegistration, DateTimeOffset UpdatedAt);
public sealed record CredentialResponse(string ModuleKey, string Label, string Username, string Password);
public sealed record CompanyDetail(Guid Id, string CompanyName, string Cnpj, string? StateRegistration, IReadOnlyCollection<CredentialResponse> Credentials, DateTimeOffset UpdatedAt);

public interface ICompanyService
{
    Task<CompanySummary> CreateAsync(Guid organizationId, CreateCompanyRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CompanySummary>> SearchAsync(Guid organizationId, string? search, CancellationToken cancellationToken);
    Task<CompanyDetail?> GetAsync(Guid organizationId, Guid companyId, CancellationToken cancellationToken);
    Task<CompanySummary?> UpdateAsync(Guid organizationId, Guid companyId, CreateCompanyRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid organizationId, Guid companyId, CancellationToken cancellationToken);
}
