using AccessControl.Application.Companies;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AccessControl.Api.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize]
public sealed class CompaniesController(ICompanyService companies, IValidator<CreateCompanyRequest> validator) : ControllerBase
{
    // The tenant boundary is a signed JWT claim; client-provided organization IDs would allow impersonation.
    private Guid OrganizationId => Guid.Parse(User.FindFirst("organization_id")?.Value ?? throw new UnauthorizedAccessException());
    private Guid ActorUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());

    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<CompanySummary>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyCollection<CompanySummary>> Search([FromQuery] string? search, CancellationToken cancellationToken) => companies.SearchAsync(OrganizationId, search, cancellationToken);

    [HttpGet("{companyId:guid}")]
    [ProducesResponseType<CompanyDetail>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyDetail>> Get(Guid companyId, CancellationToken cancellationToken)
    {
        var company = await companies.GetAsync(OrganizationId, ActorUserId, companyId, cancellationToken);
        return company is null ? NotFound() : Ok(company);
    }

    [HttpPost]
    [ProducesResponseType<CompanySummary>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CompanySummary>> Create(CreateCompanyRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(new ValidationProblemDetails(validation.ToDictionary()));
        }
        var company = await companies.CreateAsync(OrganizationId, ActorUserId, request, cancellationToken);
        return CreatedAtAction(nameof(Search), new { company.Id }, company);
    }

    [HttpPut("{companyId:guid}")]
    [ProducesResponseType<CompanySummary>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CompanySummary>> Update(Guid companyId, CreateCompanyRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return BadRequest(new ValidationProblemDetails(validation.ToDictionary()));
        var company = await companies.UpdateAsync(OrganizationId, ActorUserId, companyId, request, cancellationToken);
        return company is null ? NotFound() : Ok(company);
    }

    [HttpDelete("{companyId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid companyId, CancellationToken cancellationToken) => await companies.DeleteAsync(OrganizationId, ActorUserId, companyId, cancellationToken) ? NoContent() : NotFound();
}
