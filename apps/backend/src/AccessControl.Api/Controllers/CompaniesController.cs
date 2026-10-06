using AccessControl.Application.Companies;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AccessControl.Api.Controllers;

[ApiController]
[Route("api/companies")]
public sealed class CompaniesController(ICompanyService companies, IValidator<CreateCompanyRequest> validator) : ControllerBase
{
    // Authentication will supply this organization claim. A header is deliberately not accepted in production.
    private static readonly Guid DevelopmentOrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<CompanySummary>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyCollection<CompanySummary>> Search([FromQuery] string? search, CancellationToken cancellationToken) => companies.SearchAsync(DevelopmentOrganizationId, search, cancellationToken);

    [HttpPost]
    [ProducesResponseType<CompanySummary>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CompanySummary>> Create(CreateCompanyRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return ValidationProblem(validation.ToDictionary());
        var company = await companies.CreateAsync(DevelopmentOrganizationId, request, cancellationToken);
        return CreatedAtAction(nameof(Search), new { company.Id }, company);
    }
}
