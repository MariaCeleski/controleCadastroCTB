using AccessControl.Application.Users;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessControl.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Administrator")]
public sealed class UsersController(IUserManagementService users, IValidator<CreateUserRequest> validator) : ControllerBase
{
    private Guid OrganizationId => Guid.Parse(User.FindFirst("organization_id")?.Value ?? throw new UnauthorizedAccessException());

    [HttpGet]
    public Task<IReadOnlyCollection<UserSummary>> List(CancellationToken cancellationToken) => users.ListAsync(OrganizationId, cancellationToken);

    [HttpPost]
    [ProducesResponseType<UserSummary>(StatusCodes.Status201Created)]
    public async Task<ActionResult<UserSummary>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return BadRequest(new ValidationProblemDetails(validation.ToDictionary()));
        var user = await users.CreateAsync(OrganizationId, request, cancellationToken);
        return CreatedAtAction(nameof(List), new { user.Id }, user);
    }

    [HttpPatch("{userId:guid}/active")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetActive(Guid userId, [FromBody] bool isActive, CancellationToken cancellationToken) => await users.SetActiveAsync(OrganizationId, userId, isActive, cancellationToken) ? NoContent() : NotFound();
}
