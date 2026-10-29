using Auth.BusinessContract.DTOs;
using Auth.BusinessContract.Services;
using Common.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers
{
  [Route("api/[controller]/[action]")]
  [ApiController]
  public class OtpController(
    IOtpService otpService) : ControllerBase
  {
    // Send OTP to the email
    [HttpPost]
    public async Task<IServiceResult> Send(
      [FromBody] SendOTPVerificationRequest sendOTPVerificationRequest,
      CancellationToken cancellationToken)
    {
      var result = await otpService.SendAsync(sendOTPVerificationRequest, cancellationToken);
      return result;
    }

    // Verify OTP = authenticate, returns the same result as sign in
    [HttpPost]
    public async Task<IServiceResult> Verify(
      [FromBody] VerifyOTPRequest verifyOTPRequest,
      CancellationToken cancellationToken)
    {
      var result = await otpService.VerifyAsync(verifyOTPRequest, cancellationToken);
      return result;
    }
  }
}
