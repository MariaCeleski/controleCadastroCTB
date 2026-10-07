using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace AccessControl.IntegrationTests;

[CollectionDefinition(nameof(ApiIntegrationFixture), DisableParallelization = true)]
public sealed class ApiIntegrationFixture : ICollectionFixture<AccessControlApiFactory> { }

[Collection(nameof(ApiIntegrationFixture))]
public sealed class ApiIntegrationTests(AccessControlApiFactory factory) : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public Task DisposeAsync() => Task.CompletedTask;
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    [Fact]
    public async Task CompaniesEndpointRejectsUnauthenticatedRequests()
    {
        var response = await factory.CreateClient().GetAsync("/api/companies");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CompanyFromAnotherOrganizationIsNotAccessible()
    {
        var firstClient = await RegisterAndAuthenticateAsync("Organização Um", "admin-um@exemplo.com");
        var secondClient = await RegisterAndAuthenticateAsync("Organização Dois", "admin-dois@exemplo.com");
        var created = await CreateCompanyAsync(secondClient, "Empresa da Organização Dois", "11222333000181");

        var response = await firstClient.GetAsync($"/api/companies/{created.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SensitiveActionsAreRecordedWithoutCredentialValues()
    {
        var client = await RegisterAndAuthenticateAsync("Organização Auditada", "admin-auditoria@exemplo.com");
        var created = await CreateCompanyAsync(client, "Empresa Auditada", "11222333000181");
        var companyResponse = await client.GetAsync($"/api/companies/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, companyResponse.StatusCode);

        var auditResponse = await client.GetAsync("/api/audit-events");
        var payload = await auditResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, auditResponse.StatusCode);
        Assert.Contains("OrganizationRegistered", payload, StringComparison.Ordinal);
        Assert.Contains("UserAuthenticated", payload, StringComparison.Ordinal);
        Assert.Contains("CompanyCreated", payload, StringComparison.Ordinal);
        Assert.Contains("CompanyCredentialsViewed", payload, StringComparison.Ordinal);
        Assert.DoesNotContain("senha-secreta-de-teste", payload, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SearchByRegistrationNumberReturnsCompany()
    {
        var client = await RegisterAndAuthenticateAsync("Organização de Busca", "admin-busca@exemplo.com");
        await CreateCompanyAsync(client, "Empresa Pesquisável", "11222333000181", "000123");

        var response = await client.GetAsync("/api/companies?search=000123");
        var payload = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Empresa Pesquisável", payload, StringComparison.Ordinal);
        Assert.Contains("000123", payload, StringComparison.Ordinal);
    }

    private async Task<HttpClient> RegisterAndAuthenticateAsync(string organizationName, string email)
    {
        var client = factory.CreateClient();
        var password = "SenhaDeTeste#123";
        var registrationResponse = await client.PostAsJsonAsync("/api/auth/register-organization", new
        {
            organizationName,
            name = "Administrador de Teste",
            email,
            password,
        });
        Assert.Equal(HttpStatusCode.NoContent, registrationResponse.StatusCode);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        loginResponse.EnsureSuccessStatusCode();
        var session = await loginResponse.Content.ReadFromJsonAsync<SessionResponse>(JsonOptions);
        Assert.NotNull(session);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        return client;
    }

    private static async Task<CompanyResponse> CreateCompanyAsync(HttpClient client, string companyName, string cnpj, string registrationNumber = "000123")
    {
        var response = await client.PostAsJsonAsync("/api/companies", new
        {
            companyName,
            registrationNumber,
            cnpj,
            stateRegistration = "123456789",
            credentials = new[]
            {
                new { moduleKey = "fiscal", label = "Fiscal", username = "usuario-fiscal", password = "senha-secreta-de-teste" },
                new { moduleKey = "financeiro", label = "Financeiro", username = "usuario-financeiro", password = "senha-secreta-de-teste" },
                new { moduleKey = "trabalhista", label = "Trabalhista", username = "usuario-trabalhista", password = "senha-secreta-de-teste" },
                new { moduleKey = "portal", label = "Portal", username = "usuario-portal", password = "senha-secreta-de-teste" },
            },
        });
        response.EnsureSuccessStatusCode();
        var company = await response.Content.ReadFromJsonAsync<CompanyResponse>(JsonOptions);
        return Assert.IsType<CompanyResponse>(company);
    }

    private sealed record SessionResponse(string AccessToken);
    private sealed record CompanyResponse(Guid Id);
}
