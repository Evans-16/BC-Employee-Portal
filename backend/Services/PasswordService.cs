namespace EmployeePortal.Services;

/// <summary>
/// BCrypt-backed password service.
/// WorkFactor 12 is a good balance of security vs. server load for 2025.
/// Increase to 13-14 when you move to a more powerful host.
/// </summary>
public class PasswordService : IPasswordService
{
    private const int WorkFactor = 12;

    public string Hash(string plaintext)
    {
        if (string.IsNullOrWhiteSpace(plaintext))
            throw new ArgumentException("Password cannot be empty.", nameof(plaintext));

        return BCrypt.Net.BCrypt.HashPassword(plaintext, WorkFactor);
    }

    public bool Verify(string plaintext, string hash)
    {
        if (string.IsNullOrWhiteSpace(plaintext) || string.IsNullOrWhiteSpace(hash))
            return false;

        return BCrypt.Net.BCrypt.Verify(plaintext, hash);
    }
}