using AccessControl.Application.Auditing;
using AccessControl.Application.Security;
using AccessControl.Domain.Entities;
using AccessControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Infrastructure.Services;

public sealed class IdentityService(AccessControlDbContext db, IPasswordHasher passwords, ITokenService tokens, IAuditEventService auditEvents) : IIdentityService
{
    public async Task RegisterOrganizationAsync(RegisterOrganizationRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email, cancellationToken)) throw new InvalidOperationException("Este e-mail já está cadastrado.");
        var organization = new Organization(request.OrganizationName);
        db.Organizations.Add(organization);
        var administrator = new User(organization.Id, request.Name, email, passwords.Hash(request.Password), UserRole.Administrator);
        db.Users.Add(administrator);
        auditEvents.Record(organization.Id, administrator.Id, AuditAction.OrganizationRegistered, nameof(Organization), organization.Id);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuthenticatedSession?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email && x.IsActive, cancellationToken);
        if (user is null || !passwords.Verify(request.Password, user.PasswordHash)) return null;
        auditEvents.Record(user.OrganizationId, user.Id, AuditAction.UserAuthenticated, nameof(User), user.Id);
        await db.SaveChangesAsync(cancellationToken);
        return tokens.Create(user.Id, user.OrganizationId, user.Name, user.Role.ToString());
    }
}
