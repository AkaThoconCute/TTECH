using Auth.BusinessContract.DTOs;
using Auth.BusinessContract.Services;
using Common.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers
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
  }
}
