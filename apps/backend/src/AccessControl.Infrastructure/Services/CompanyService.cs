using AccessControl.Application.Companies;
using AccessControl.Application.Security;
using AccessControl.Domain.Entities;
using AccessControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Infrastructure.Services;

public sealed class CompanyService(AccessControlDbContext db, ICredentialCipher cipher) : ICompanyService
{
    public async Task<CompanySummary> CreateAsync(Guid organizationId, CreateCompanyRequest request, CancellationToken cancellationToken)
    {
        var cnpj = new string(request.Cnpj.Where(char.IsDigit).ToArray());
        var exists = await db.Companies.AnyAsync(x => x.OrganizationId == organizationId && x.CnpjDigits == cnpj, cancellationToken);
        if (exists) throw new InvalidOperationException("Já existe uma empresa com este CNPJ.");
        var company = new Company(organizationId, request.CompanyName, cnpj, request.StateRegistration);
        company.ReplaceCredentials(request.Credentials.Select(x => new AccessCredential(company.Id, x.ModuleKey, x.Label, x.Username, cipher.Encrypt(x.Password))));
        db.Companies.Add(company);
        await db.SaveChangesAsync(cancellationToken);
        return new CompanySummary(company.Id, company.CompanyName, company.CnpjDigits, company.StateRegistration, company.UpdatedAt);
    }

    public async Task<IReadOnlyCollection<CompanySummary>> SearchAsync(Guid organizationId, string? search, CancellationToken cancellationToken)
    {
        var query = db.Companies.AsNoTracking().Where(x => x.OrganizationId == organizationId);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.CompanyName.Contains(search) || x.CnpjDigits.Contains(search));
        return await query.OrderBy(x => x.CompanyName).Take(50).Select(x => new CompanySummary(x.Id, x.CompanyName, x.CnpjDigits, x.StateRegistration, x.UpdatedAt)).ToListAsync(cancellationToken);
    }
}
