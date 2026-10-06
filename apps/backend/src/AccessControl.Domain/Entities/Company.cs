namespace AccessControl.Domain.Entities;

public sealed class Company : Entity
{
    private readonly List<AccessCredential> _credentials = [];

    public Guid OrganizationId { get; private set; }
    public string CompanyName { get; private set; } = string.Empty;
    public string CnpjDigits { get; private set; } = string.Empty;
    public string? StateRegistration { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public IReadOnlyCollection<AccessCredential> Credentials => _credentials.AsReadOnly();

    private Company() { }

    public Company(Guid organizationId, string companyName, string cnpjDigits, string? stateRegistration)
    {
        OrganizationId = organizationId;
        Update(companyName, cnpjDigits, stateRegistration);
    }

    public void Update(string companyName, string cnpjDigits, string? stateRegistration)
    {
        CompanyName = companyName.Trim();
        CnpjDigits = cnpjDigits;
        StateRegistration = string.IsNullOrWhiteSpace(stateRegistration) ? null : stateRegistration.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ReplaceCredentials(IEnumerable<AccessCredential> credentials)
    {
        _credentials.Clear();
        _credentials.AddRange(credentials);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
