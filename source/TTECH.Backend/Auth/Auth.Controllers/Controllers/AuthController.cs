using Auth.BusinessContract.DTO;
using Auth.BusinessContract.Service;
using Common.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.Controllers
{
  [Route("api/[controller]/[action]")]
  [ApiController]
  public class AuthController(
    IAuthService authService) : ControllerBase
  {
    // Sign up API
    [HttpPost]
    public async Task<IServiceResult> SignUp([FromBody] SignUpRequest signUpRequest)
    {
      var result = await authService.SignUpAsync(signUpRequest);
      return result;
    }

    // Login API
    [HttpPost]
    public async Task<IServiceResult> SignIn([FromBody] SignInRequest signInRequest)
    {
      var result = await authService.SignInAsync(signInRequest);
      return result;
    }

    #region Verify OTP API
    [HttpPost]
    public async Task<IServiceResult> SendOTPVerificationAsync([FromBody] SendOTPVerificationRequest sendOTPVerificationRequest)
    {
      var result = await authService.SendOTPVerificationAsync(sendOTPVerificationRequest);
      return result;
    }

    [HttpPost]
    public async Task<IServiceResult> VerifyOTPAsync([FromBody] VerifyOTPRequest verifyOTPRequest)
    {
      var result = await authService.VerifyOTPAsync(verifyOTPRequest);
      return result;
    }
    #endregion

  }
}
