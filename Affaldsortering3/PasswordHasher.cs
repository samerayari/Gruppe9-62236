using System.Security.Cryptography;

namespace Affaldsortering3;

// Denne klasse bruges til at beskytte passwords
// Den sørger for at passwords aldrig gemmes som almindelig tekst
public class PasswordHasher
{
    // Laver et salt og et hash ud fra et password
    // Bruges når en ny bruger oprettes
    public (byte[] salt, byte[] hash) Hash(string password)
    {
        // Laver et tilfældigt salt (16 bytes)
        var salt = RandomNumberGenerator.GetBytes(16);

        // Laver et sikkert hash ud fra password + salt
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations: 100_000,                 // Gør det langsomt for hackere
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: 32);

        // Returnerer både salt og hash
        return (salt, hash);
    }

    // Tjekker om et indtastet password matcher det gemte password
    // Bruges ved login
    public bool Verify(string password, byte[] salt, byte[] expectedHash)
    {
        // Laver et nyt hash ud fra det indtastede password
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations: 100_000,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: 32);

        // Sammenligner de to hashes på en sikker måde
        return CryptographicOperations.FixedTimeEquals(hash, expectedHash);
    }
}