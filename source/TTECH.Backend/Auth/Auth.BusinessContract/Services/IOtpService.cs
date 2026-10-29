using Auth.BusinessContract.DTOs;
using Common.DTO;

namespace Auth.BusinessContract.Services
{
  // Purpose-scoped email OTP. Verifying a code authenticates the user and returns tokens like sign in.
  public interface IOtpService
  {
    Task<ServiceResult<SendOTPVerificationResponse>> SendAsync(
      SendOTPVerificationRequest sendOTPVerificationRequest,
      CancellationToken cancellationToken = default);

    Task<ServiceResult<TokensResponse>> VerifyAsync(
      VerifyOTPRequest verifyOTPRequest,
      CancellationToken cancellationToken = default);

    // Deletes OTP records that expired long ago. Meant to be run on a schedule.
    Task CleanupExpiredAsync(CancellationToken cancellationToken = default);
  }
}
