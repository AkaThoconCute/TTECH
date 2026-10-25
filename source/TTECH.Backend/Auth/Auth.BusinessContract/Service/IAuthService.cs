using Auth.BusinessContract.DTO;
using Common.DTO;

namespace Auth.BusinessContract.Service
{
  public interface IAuthService
  {
    public Task<ServiceResult<TokensResponse>> SignUpAsync(SignUpRequest signUpRequest);
    public Task<ServiceResult<TokensResponse>> SignInAsync(SignInRequest logInRequest);
  }
}
