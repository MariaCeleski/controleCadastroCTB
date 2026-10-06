using System.Security.Cryptography;
using System.Text;
using AccessControl.Application.Security;

namespace AccessControl.Infrastructure.Services;

// AES-GCM provides authenticated encryption. The 32-byte key is injected via a secret store, never source control.
public sealed class AesCredentialCipher(string base64Key) : ICredentialCipher
{
    private readonly byte[] _key = Convert.FromBase64String(base64Key);

    public string Encrypt(string plaintext)
    {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var tag = new byte[16];
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        using var aes = new AesGcm(_key, tag.Length);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);
        return Convert.ToBase64String([.. nonce, .. tag, .. ciphertext]);
    }

    public string Decrypt(string ciphertext)
    {
        var payload = Convert.FromBase64String(ciphertext);
        var nonce = payload[..12];
        var tag = payload[12..28];
        var encrypted = payload[28..];
        var plaintext = new byte[encrypted.Length];
        using var aes = new AesGcm(_key, tag.Length);
        aes.Decrypt(nonce, encrypted, tag, plaintext);
        return Encoding.UTF8.GetString(plaintext);
    }
}
