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
        // Always include the organization boundary: it prevents cross-customer data access.
        var exists = await db.Companies.AnyAsync(x => x.OrganizationId == organizationId && x.CnpjDigits == cnpj, cancellationToken);
        if (exists) throw new InvalidOperationException("Já existe uma empresa com este CNPJ.");
        var company = new Company(organizationId, request.CompanyName, cnpj, request.StateRegistration);
        company.ReplaceCredentials(request.Credentials.Select(x => new AccessCredential(company.Id, x.ModuleKey, x.Label, x.Username, cipher.Encrypt(x.Password))));
        db.Companies.Add(company);
        // Credentials are explicitly persisted because the domain collection is deliberately read-only to EF.
        db.Credentials.AddRange(company.Credentials);
        await db.SaveChangesAsync(cancellationToken);
        return new CompanySummary(company.Id, company.CompanyName, company.CnpjDigits, company.StateRegistration, company.UpdatedAt);
    }

    public async Task<IReadOnlyCollection<CompanySummary>> SearchAsync(Guid organizationId, string? search, CancellationToken cancellationToken)
    {
        var query = db.Companies.AsNoTracking().Where(x => x.OrganizationId == organizationId);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.CompanyName.Contains(search) || x.CnpjDigits.Contains(search));
        return await query.OrderBy(x => x.CompanyName).Take(50).Select(x => new CompanySummary(x.Id, x.CompanyName, x.CnpjDigits, x.StateRegistration, x.UpdatedAt)).ToListAsync(cancellationToken);
    }

    public async Task<CompanyDetail?> GetAsync(Guid organizationId, Guid companyId, CancellationToken cancellationToken)
    {
        var company = await db.Companies.AsNoTracking().SingleOrDefaultAsync(x => x.Id == companyId && x.OrganizationId == organizationId, cancellationToken);
        if (company is null) return null;
        var credentials = await db.Credentials.AsNoTracking().Where(x => x.CompanyId == companyId).OrderBy(x => x.ModuleKey).ToListAsync(cancellationToken);
        return new CompanyDetail(company.Id, company.CompanyName, company.CnpjDigits, company.StateRegistration, credentials.Select(x => new CredentialResponse(x.ModuleKey, x.Label, x.Username, cipher.Decrypt(x.EncryptedPassword))).ToList(), company.UpdatedAt);
    }

    public async Task<CompanySummary?> UpdateAsync(Guid organizationId, Guid companyId, CreateCompanyRequest request, CancellationToken cancellationToken)
    {
        var company = await db.Companies.SingleOrDefaultAsync(x => x.Id == companyId && x.OrganizationId == organizationId, cancellationToken);
        if (company is null) return null;
        var cnpj = new string(request.Cnpj.Where(char.IsDigit).ToArray());
        var duplicate = await db.Companies.AnyAsync(x => x.OrganizationId == organizationId && x.CnpjDigits == cnpj && x.Id != companyId, cancellationToken);
        if (duplicate) throw new InvalidOperationException("Já existe uma empresa com este CNPJ.");
        company.Update(request.CompanyName, cnpj, request.StateRegistration);
        db.Credentials.RemoveRange(db.Credentials.Where(x => x.CompanyId == companyId));
        var credentials = request.Credentials.Select(x => new AccessCredential(companyId, x.ModuleKey, x.Label, x.Username, cipher.Encrypt(x.Password))).ToList();
        company.ReplaceCredentials(credentials);
        db.Credentials.AddRange(credentials);
        await db.SaveChangesAsync(cancellationToken);
        return new CompanySummary(company.Id, company.CompanyName, company.CnpjDigits, company.StateRegistration, company.UpdatedAt);
    }

    public async Task<bool> DeleteAsync(Guid organizationId, Guid companyId, CancellationToken cancellationToken)
    {
        var company = await db.Companies.SingleOrDefaultAsync(x => x.Id == companyId && x.OrganizationId == organizationId, cancellationToken);
        if (company is null) return false;
        db.Companies.Remove(company);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
