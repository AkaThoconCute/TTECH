using System.ComponentModel.DataAnnotations;

namespace Auth.BusinessImplement.Options
{
  public class OtpOptions
  {
    public const string SectionName = "Otp";

    // Secret used to HMAC codes and verification tokens. Keep it out of source control (user-secrets / env var).
    [Required, MinLength(32)]
    public string HashKey { get; init; } = string.Empty;

    [Range(1, 60)]
    public int ExpiryMinutes { get; init; } = 5;

    [Range(1, 10)]
    public int MaxAttempts { get; init; } = 5;

    [Range(0, 3600)]
    public int ResendCooldownSeconds { get; init; } = 60;

    // How often the cleanup job runs
    [Range(1, 1440)]
    public int CleanupIntervalMinutes { get; init; } = 60;

    // Expired codes are kept this long before they are deleted
    [Range(0, 720)]
    public int CleanupRetentionHours { get; init; } = 24;

    [Required]
    public string SystemName { get; init; } = "TTECH";

    [Required]
    public string SupportUrl { get; init; } = "https://example.com/support";
  }
}
