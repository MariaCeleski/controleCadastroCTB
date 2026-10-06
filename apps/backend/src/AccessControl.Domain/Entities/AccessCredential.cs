namespace AccessControl.Domain.Entities;

public sealed class AccessCredential : Entity
{
    public Guid CompanyId { get; private set; }
    public string ModuleKey { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    // This field must only ever hold ciphertext created by ICredentialCipher.
    public string EncryptedPassword { get; private set; } = string.Empty;

    private AccessCredential() { }

    public AccessCredential(Guid companyId, string moduleKey, string label, string username, string encryptedPassword)
    {
        CompanyId = companyId;
        ModuleKey = moduleKey;
        Label = label;
        Username = username;
        EncryptedPassword = encryptedPassword;
    }
}
