using AccessControl.Application.Auditing;
using AccessControl.Domain.Entities;
using AccessControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Infrastructure.Services;

public sealed class AuditEventService(AccessControlDbContext db) : IAuditEventService
{
    public void Record(Guid organizationId, Guid? actorUserId, AuditAction action, string entityType, Guid? entityId) =>
        db.AuditEvents.Add(new AuditEvent(organizationId, actorUserId, action, entityType, entityId));

    public async Task<IReadOnlyCollection<AuditEventResponse>> ListAsync(Guid organizationId, CancellationToken cancellationToken) =>
        await db.AuditEvents.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.OccurredAt)
            .Take(100)
            .Select(x => new AuditEventResponse(x.Id, x.ActorUserId, x.Action, x.EntityType, x.EntityId, x.OccurredAt))
            .ToListAsync(cancellationToken);
}
