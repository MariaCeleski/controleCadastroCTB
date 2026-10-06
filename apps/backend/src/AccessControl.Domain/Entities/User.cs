namespace AccessControl.Domain.Entities;

public enum UserRole { Administrator, Operator }

public sealed class User : Entity
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;

    private User() { }
    public User(Guid organizationId, string name, string email, string passwordHash, UserRole role)
    {
        OrganizationId = organizationId;
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role;
    }
}
