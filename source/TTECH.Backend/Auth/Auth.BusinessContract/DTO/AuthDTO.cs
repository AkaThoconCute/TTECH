using System.ComponentModel.DataAnnotations;

namespace Auth.BusinessContract.DTO
{
  // SignupRequest
  public class SignUpRequest
  {
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, digits, and underscores.")]
    public string Username { get; init; } = string.Empty;

    //[Required(ErrorMessage = "Password is required.")]
    //[MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    //[RegularExpression(@"^\d+$", ErrorMessage = "Password must contain only numbers.")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Full Name can only contain letters, digits and spaces.")]
    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;
  }

  // LoginRequest
  public class SignInRequest
  {
    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
  }

  // SendOTPVerificationRequest
  public class SendOTPVerificationRequest
  {
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; init; } = string.Empty;
  }

  // VerifyOTPRequest
  public class VerifyOTPRequest
  {
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "OTP is required.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 characters.")]
    public string OTP { get; init; } = string.Empty;
  }
}
