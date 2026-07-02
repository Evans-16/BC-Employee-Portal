namespace EmployeePortal.Services;

/// <summary>
/// Abstracts password hashing so the algorithm can be swapped without
/// touching controllers or BcService.
/// </summary>
public interface IPasswordService
{
    /// <summary>Hashes a plaintext password for storage in BC.</summary>
    string Hash(string plaintext);

    /// <summary>Verifies a plaintext password against a stored hash.</summary>
    bool Verify(string plaintext, string hash);
}