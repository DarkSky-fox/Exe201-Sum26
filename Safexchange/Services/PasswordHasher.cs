using System.Security.Cryptography;
using System.Text;

namespace Safexchange.Services;

public static class PasswordHasher
{
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public static string HashGooglePlaceholder(string email)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes("GOOGLE_OAUTH:" + email));
        return Convert.ToBase64String(bytes);
    }

    public static bool IsGoogleAccount(string email, string passwordHash)
        => passwordHash == HashGooglePlaceholder(email);
}
