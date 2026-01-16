using System.Security.Cryptography;

namespace Affaldsortering3;

public class PasswordHasher
{
    public (byte[] salt, byte[] hash) Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations: 100_000,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: 32);

        return (salt, hash);
    }

    public bool Verify(string password, byte[] salt, byte[] expectedHash)
    {
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations: 100_000,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: 32);

        return CryptographicOperations.FixedTimeEquals(hash, expectedHash);
    }
}