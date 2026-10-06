using AccessControl.Application.Security;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AccessControl.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IIdentityService identity, IValidator<RegisterOrganizationRequest> registrationValidator, IValidator<LoginRequest> loginValidator) : ControllerBase
{
    [HttpPost("register-organization")]
    public async Task<IActionResult> RegisterOrganization(RegisterOrganizationRequest request, CancellationToken cancellationToken)
    {
        var validation = await registrationValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return BadRequest(new ValidationProblemDetails(validation.ToDictionary()));
        await identity.RegisterOrganizationAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthenticatedSession>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticatedSession>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var validation = await loginValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return BadRequest(new ValidationProblemDetails(validation.ToDictionary()));
        var session = await identity.LoginAsync(request, cancellationToken);
        return session is null ? Unauthorized() : Ok(session);
    }
}
