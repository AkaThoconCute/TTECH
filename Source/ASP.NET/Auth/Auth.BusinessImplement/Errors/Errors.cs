using Common.DTO;
using Microsoft.AspNetCore.Http;

namespace Auth.BusinessImplement.Errors
{
  public class Errors
  {
    public static readonly CustomError SignUpFailed = new()
    {
      Code = 2001,
      Message = "Failed to log in the account. Please check your info and try again.",
      HttpStatusCode = StatusCodes.Status400BadRequest
    };

    public static readonly CustomError SignUpError = new()
    {
      Code = 2001,
      Message = "Unable to log in due to a technical error. Service will be back soon.",
      HttpStatusCode = StatusCodes.Status500InternalServerError
    };

    public static readonly CustomError LogInFailed = new()
    {
      Code = 2002,
      Message = "Failed to log in the account. Please check your info and try again.",
      HttpStatusCode = StatusCodes.Status401Unauthorized
    };

    public static readonly CustomError LogInError = new()
    {
      Code = 2002,
      Message = "Unable to log in due to a technical error. Service will be back soon.",
      HttpStatusCode = StatusCodes.Status500InternalServerError
    };

    // Deliberately vague: never reveal whether the email exists, the code expired, or attempts ran out.
    public static readonly CustomError InvalidOTP = new()
    {
      Code = 2003,
      Message = "The verification code is invalid or has expired. Please request a new code.",
      HttpStatusCode = StatusCodes.Status400BadRequest
    };
  }
}
