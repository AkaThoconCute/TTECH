using System.Text.Json.Serialization;

namespace Auth.BusinessContract.Enums
{
  // Each purpose is an isolated OTP scope: a code issued for one purpose is never valid for another.
  // Add new targets (e.g. ChangeEmail) here and map their email wording in OtpServiceImpl.
  [JsonConverter(typeof(JsonStringEnumConverter))]
  public enum OtpPurposeEnum
  {
    PasswordReset = 1
  }
}
