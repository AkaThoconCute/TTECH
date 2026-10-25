using Auth.BusinessContract.Enum;
using System.ComponentModel.DataAnnotations;

namespace Auth.BusinessContract.DTO
{
  // SendOTPVerificationRequest
  public class SendOTPVerificationRequest
  {
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
    public string Email { get; init; } = string.Empty;

    [EnumDataType(typeof(OtpPurposeEnum), ErrorMessage = "Invalid OTP purpose.")]
    public OtpPurposeEnum Purpose { get; init; }
  }

  // SendOTPVerificationResponse
  // Same shape whether or not the email belongs to an account (prevents account enumeration).
  public class SendOTPVerificationResponse
  {
    public int ExpiresInSeconds { get; init; }
    public int ResendAfterSeconds { get; init; }
  }

  // VerifyOTPRequest
  // Verifying an OTP is an authentication method: the response is TokensResponse, same as sign in.
  public class VerifyOTPRequest
  {
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
    public string Email { get; init; } = string.Empty;

    [EnumDataType(typeof(OtpPurposeEnum), ErrorMessage = "Invalid OTP purpose.")]
    public OtpPurposeEnum Purpose { get; init; }

    [Required(ErrorMessage = "OTP is required.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be exactly 6 digits.")]
    public string OTP { get; init; } = string.Empty;
  }
}
