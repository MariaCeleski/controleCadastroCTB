using AccessControl.Domain.Entities;

namespace AccessControl.Application.Auditing;

public sealed record AuditEventResponse(Guid Id, Guid? ActorUserId, AuditAction Action, string EntityType, Guid? EntityId, DateTimeOffset OccurredAt);

public interface IAuditEventService
{
    void Record(Guid organizationId, Guid? actorUserId, AuditAction action, string entityType, Guid? entityId);
    Task<IReadOnlyCollection<AuditEventResponse>> ListAsync(Guid organizationId, CancellationToken cancellationToken);
}
