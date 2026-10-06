using AccessControl.Application.Auditing;
using AccessControl.Application.Security;
using AccessControl.Application.Users;
using AccessControl.Domain.Entities;
using AccessControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Infrastructure.Services;

public sealed class UserManagementService(AccessControlDbContext db, IPasswordHasher passwords, IAuditEventService auditEvents) : IUserManagementService
{
    public async Task<IReadOnlyCollection<UserSummary>> ListAsync(Guid organizationId, CancellationToken cancellationToken) => await db.Users.AsNoTracking().Where(x => x.OrganizationId == organizationId).OrderBy(x => x.Name).Select(x => new UserSummary(x.Id, x.Name, x.Email, x.Role, x.IsActive)).ToListAsync(cancellationToken);

    public async Task<UserSummary> CreateAsync(Guid organizationId, Guid actorUserId, CreateUserRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email, cancellationToken)) throw new InvalidOperationException("Este e-mail já está cadastrado.");
        var user = new User(organizationId, request.Name, email, passwords.Hash(request.Password), request.Role);
        db.Users.Add(user);
        auditEvents.Record(organizationId, actorUserId, AuditAction.UserCreated, nameof(User), user.Id);
        await db.SaveChangesAsync(cancellationToken);
        return new UserSummary(user.Id, user.Name, user.Email, user.Role, user.IsActive);
    }

    public async Task<bool> SetActiveAsync(Guid organizationId, Guid actorUserId, Guid userId, bool isActive, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Id == userId && x.OrganizationId == organizationId, cancellationToken);
        if (user is null) return false;
        user.SetActive(isActive);
        auditEvents.Record(organizationId, actorUserId, AuditAction.UserActivationChanged, nameof(User), userId);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
