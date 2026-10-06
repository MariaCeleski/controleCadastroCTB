using AccessControl.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AccessControl.IntegrationTests;

public sealed class AccessControlApiFactory : WebApplicationFactory<Program>
{
    private const string TestConnectionString = "Host=localhost;Port=5432;Database=access_control_test;Username=postgres;Password=postgres";

    public AccessControlApiFactory()
    {
        // Top-level startup reads required settings before WebApplicationFactory customizations run.
        // These variables exist only in the test process and never represent deployment secrets.
        Environment.SetEnvironmentVariable("ConnectionStrings__AccessControl", Environment.GetEnvironmentVariable("ACCESS_CONTROL_TEST_CONNECTION_STRING") ?? TestConnectionString);
        Environment.SetEnvironmentVariable("Security__CredentialEncryptionKey", "MDEyMzQ1Njc4OWFiY2RlZjAxMjM0NTY3ODlhYmNkZWY=");
        Environment.SetEnvironmentVariable("Security__Jwt__Issuer", "access-control-integration-tests");
        Environment.SetEnvironmentVariable("Security__Jwt__Audience", "access-control-integration-tests");
        Environment.SetEnvironmentVariable("Security__Jwt__SigningKey", "integration-test-signing-key-that-is-long-enough-for-hs256");
        Environment.SetEnvironmentVariable("Logging__LogLevel__Default", "Warning");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    // The dedicated test database is reset between scenarios, keeping tenant and audit assertions independent.
    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AccessControlDbContext>();
        await db.Database.MigrateAsync();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE audit_events, access_credentials, companies, users, organizations CASCADE;");
    }
}
