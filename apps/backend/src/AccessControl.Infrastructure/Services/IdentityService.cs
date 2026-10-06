using AccessControl.Application.Security;
using AccessControl.Domain.Entities;
using AccessControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Infrastructure.Services;

public sealed class IdentityService(AccessControlDbContext db, IPasswordHasher passwords, ITokenService tokens) : IIdentityService
{
    public async Task RegisterOrganizationAsync(RegisterOrganizationRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email, cancellationToken)) throw new InvalidOperationException("Este e-mail já está cadastrado.");
        var organization = new Organization(request.OrganizationName);
        db.Organizations.Add(organization);
        db.Users.Add(new User(organization.Id, request.Name, email, passwords.Hash(request.Password), UserRole.Administrator));
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuthenticatedSession?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email && x.IsActive, cancellationToken);
        return user is not null && passwords.Verify(request.Password, user.PasswordHash)
            ? tokens.Create(user.Id, user.OrganizationId, user.Name, user.Role.ToString()) : null;
    }
}
