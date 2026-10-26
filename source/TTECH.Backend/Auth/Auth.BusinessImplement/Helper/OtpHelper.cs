using Auth.BusinessContract.Enum;
using System.Security.Cryptography;
using System.Text;

namespace Auth.BusinessImplement.Helper
{
  // Low-level OTP tools: normalization, code generation, hashing, comparison.
  public static class OtpHelper
  {
    public const int CodeLength = 6;

    // Same normalization as Identity's normalized email
    public static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    // Uniform, unbiased code from a CSPRNG
    public static string GenerateCode()
      => RandomNumberGenerator.GetInt32(0, (int)Math.Pow(10, CodeLength)).ToString($"D{CodeLength}");

    // HMAC binds the secret to purpose + email, so a hash from one context can't be replayed in another
    public static string ComputeHash(string key, OtpPurposeEnum purpose, string normalizedEmail, string code)
    {
      byte[] keyBytes = Encoding.UTF8.GetBytes(key);
      byte[] data = Encoding.UTF8.GetBytes($"{(int)purpose}:{normalizedEmail}:{code}");
      return Convert.ToHexString(HMACSHA256.HashData(keyBytes, data));
    }

    public static bool HashEquals(string a, string b)
      => CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
  }
}
