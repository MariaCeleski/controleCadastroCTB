namespace AccessControl.Application.Security;

public sealed record RegisterOrganizationRequest(string OrganizationName, string Name, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthenticatedSession(string AccessToken, DateTimeOffset ExpiresAt, string UserName, string Role);

public interface IIdentityService
{
    Task RegisterOrganizationAsync(RegisterOrganizationRequest request, CancellationToken cancellationToken);
    Task<AuthenticatedSession?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string encodedHash);
}

public interface ITokenService
{
    AuthenticatedSession Create(Guid userId, Guid organizationId, string name, string role);
}
