namespace Roller.Infrastructure.Security
{
    public interface IAesEncryptionService
    {
        string Encrypt(string plain, string? aesKey = null);

        string Decrypt(string cipher, string? aesKey = null);
    }
}