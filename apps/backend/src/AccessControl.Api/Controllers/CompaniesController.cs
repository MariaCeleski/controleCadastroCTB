using AccessControl.Application.Companies;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessControl.Api.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize]
public sealed class CompaniesController(ICompanyService companies, IValidator<CreateCompanyRequest> validator) : ControllerBase
{
    private Guid OrganizationId => Guid.Parse(User.FindFirst("organization_id")?.Value ?? throw new UnauthorizedAccessException());

    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<CompanySummary>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyCollection<CompanySummary>> Search([FromQuery] string? search, CancellationToken cancellationToken) => companies.SearchAsync(OrganizationId, search, cancellationToken);

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
        var company = await companies.CreateAsync(OrganizationId, request, cancellationToken);
        return CreatedAtAction(nameof(Search), new { company.Id }, company);
    }
}
