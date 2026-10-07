using AccessControl.Application.Companies;
using AccessControl.Application.Auditing;
using AccessControl.Application.Security;
using AccessControl.Domain.Entities;
using AccessControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Infrastructure.Services;

public sealed class CompanyService(AccessControlDbContext db, ICredentialCipher cipher, IAuditEventService auditEvents) : ICompanyService
{
    public async Task<CompanySummary> CreateAsync(Guid organizationId, Guid actorUserId, CreateCompanyRequest request, CancellationToken cancellationToken)
    {
        var cnpj = new string(request.Cnpj.Where(char.IsDigit).ToArray());
        var registrationNumber = new string(request.RegistrationNumber.Where(char.IsDigit).ToArray());
        // Always include the organization boundary: it prevents cross-customer data access.
        var exists = await db.Companies.AnyAsync(x => x.OrganizationId == organizationId && x.CnpjDigits == cnpj, cancellationToken);
        if (exists) throw new InvalidOperationException("Já existe uma empresa com este CNPJ.");
        var registrationNumberExists = await db.Companies.AnyAsync(x => x.OrganizationId == organizationId && x.RegistrationNumber == registrationNumber, cancellationToken);
        if (registrationNumberExists) throw new InvalidOperationException("Já existe uma empresa com este número de cadastro.");
        var company = new Company(organizationId, registrationNumber, request.CompanyName, cnpj, request.StateRegistration);
        company.ReplaceCredentials(request.Credentials.Select(x => new AccessCredential(company.Id, x.ModuleKey, x.Label, x.Username, cipher.Encrypt(x.Password))));
        db.Companies.Add(company);
        // Credentials are explicitly persisted because the domain collection is deliberately read-only to EF.
        db.Credentials.AddRange(company.Credentials);
        auditEvents.Record(organizationId, actorUserId, AuditAction.CompanyCreated, nameof(Company), company.Id);
        await db.SaveChangesAsync(cancellationToken);
        return new CompanySummary(company.Id, company.CompanyName, company.RegistrationNumber, company.CnpjDigits, company.StateRegistration, company.UpdatedAt);
    }

    public async Task<IReadOnlyCollection<CompanySummary>> SearchAsync(Guid organizationId, string? search, CancellationToken cancellationToken)
    {
        var query = db.Companies.AsNoTracking().Where(x => x.OrganizationId == organizationId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var numericSearch = new string(search.Where(char.IsDigit).ToArray());
            query = query.Where(x => x.CompanyName.Contains(search) || (!string.IsNullOrEmpty(numericSearch) && (x.CnpjDigits.Contains(numericSearch) || x.RegistrationNumber.Contains(numericSearch))));
        }
        return await query.OrderBy(x => x.CompanyName).Take(50).Select(x => new CompanySummary(x.Id, x.CompanyName, x.RegistrationNumber, x.CnpjDigits, x.StateRegistration, x.UpdatedAt)).ToListAsync(cancellationToken);
    }

    public async Task<CompanyDetail?> GetAsync(Guid organizationId, Guid actorUserId, Guid companyId, CancellationToken cancellationToken)
    {
        var company = await db.Companies.AsNoTracking().SingleOrDefaultAsync(x => x.Id == companyId && x.OrganizationId == organizationId, cancellationToken);
        if (company is null) return null;
        var credentials = await db.Credentials.AsNoTracking().Where(x => x.CompanyId == companyId).OrderBy(x => x.ModuleKey).ToListAsync(cancellationToken);
        // Reading this endpoint decrypts credentials; its access is therefore an auditable security event.
        auditEvents.Record(organizationId, actorUserId, AuditAction.CompanyCredentialsViewed, nameof(Company), companyId);
        await db.SaveChangesAsync(cancellationToken);
        return new CompanyDetail(company.Id, company.CompanyName, company.RegistrationNumber, company.CnpjDigits, company.StateRegistration, credentials.Select(x => new CredentialResponse(x.ModuleKey, x.Label, x.Username, cipher.Decrypt(x.EncryptedPassword))).ToList(), company.UpdatedAt);
    }

    public async Task<CompanySummary?> UpdateAsync(Guid organizationId, Guid actorUserId, Guid companyId, CreateCompanyRequest request, CancellationToken cancellationToken)
    {
        var company = await db.Companies.SingleOrDefaultAsync(x => x.Id == companyId && x.OrganizationId == organizationId, cancellationToken);
        if (company is null) return null;
        var cnpj = new string(request.Cnpj.Where(char.IsDigit).ToArray());
        var registrationNumber = new string(request.RegistrationNumber.Where(char.IsDigit).ToArray());
        var duplicate = await db.Companies.AnyAsync(x => x.OrganizationId == organizationId && x.CnpjDigits == cnpj && x.Id != companyId, cancellationToken);
        if (duplicate) throw new InvalidOperationException("Já existe uma empresa com este CNPJ.");
        var duplicateRegistrationNumber = await db.Companies.AnyAsync(x => x.OrganizationId == organizationId && x.RegistrationNumber == registrationNumber && x.Id != companyId, cancellationToken);
        if (duplicateRegistrationNumber) throw new InvalidOperationException("Já existe uma empresa com este número de cadastro.");
        company.Update(registrationNumber, request.CompanyName, cnpj, request.StateRegistration);
        db.Credentials.RemoveRange(db.Credentials.Where(x => x.CompanyId == companyId));
        var credentials = request.Credentials.Select(x => new AccessCredential(companyId, x.ModuleKey, x.Label, x.Username, cipher.Encrypt(x.Password))).ToList();
        company.ReplaceCredentials(credentials);
        db.Credentials.AddRange(credentials);
        auditEvents.Record(organizationId, actorUserId, AuditAction.CompanyUpdated, nameof(Company), companyId);
        await db.SaveChangesAsync(cancellationToken);
        return new CompanySummary(company.Id, company.CompanyName, company.RegistrationNumber, company.CnpjDigits, company.StateRegistration, company.UpdatedAt);
    }

    public async Task<bool> DeleteAsync(Guid organizationId, Guid actorUserId, Guid companyId, CancellationToken cancellationToken)
    {
        var company = await db.Companies.SingleOrDefaultAsync(x => x.Id == companyId && x.OrganizationId == organizationId, cancellationToken);
        if (company is null) return false;
        auditEvents.Record(organizationId, actorUserId, AuditAction.CompanyDeleted, nameof(Company), companyId);
        db.Companies.Remove(company);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
