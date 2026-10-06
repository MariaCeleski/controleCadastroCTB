namespace AccessControl.Domain.Entities;

public sealed class Organization : Entity
{
    public string Name { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private Organization() { }
    public Organization(string name) => Name = name.Trim();
}
