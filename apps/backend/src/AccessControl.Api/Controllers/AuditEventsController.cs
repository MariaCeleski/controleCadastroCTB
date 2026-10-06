using AccessControl.Application.Auditing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessControl.Api.Controllers;

[ApiController]
[Route("api/audit-events")]
[Authorize(Roles = "Administrator")]
public sealed class AuditEventsController(IAuditEventService auditEvents) : ControllerBase
{
    // Administrators can inspect only events belonging to their own signed tenant claim.
    private Guid OrganizationId => Guid.Parse(User.FindFirst("organization_id")?.Value ?? throw new UnauthorizedAccessException());

    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<AuditEventResponse>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyCollection<AuditEventResponse>> List(CancellationToken cancellationToken) =>
        auditEvents.ListAsync(OrganizationId, cancellationToken);
}
