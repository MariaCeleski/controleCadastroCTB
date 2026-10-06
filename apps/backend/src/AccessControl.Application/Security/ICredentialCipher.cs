namespace AccessControl.Application.Security;

public interface ICredentialCipher
{
    string Encrypt(string plaintext);
    string Decrypt(string ciphertext);
}
