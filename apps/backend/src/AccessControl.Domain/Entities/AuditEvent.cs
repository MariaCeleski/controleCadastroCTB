namespace AccessControl.Domain.Entities;

public enum AuditAction
{
    OrganizationRegistered,
    UserAuthenticated,
    CompanyCreated,
    CompanyCredentialsViewed,
    CompanyUpdated,
    CompanyDeleted,
    UserCreated,
    UserActivationChanged
}

public sealed class AuditEvent : Entity
{
    public Guid OrganizationId { get; private set; }
    public Guid? ActorUserId { get; private set; }
    public AuditAction Action { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public Guid? EntityId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; } = DateTimeOffset.UtcNow;

    private AuditEvent() { }

    // Metadata is deliberately omitted: audit records must never become a secondary store for credentials or tokens.
    public AuditEvent(Guid organizationId, Guid? actorUserId, AuditAction action, string entityType, Guid? entityId)
    {
        OrganizationId = organizationId;
        ActorUserId = actorUserId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
    }
}
