using Auth.BusinessContract.DTO;
using Common.DTO;

namespace Auth.BusinessContract.Service
{
  public interface IEmailService
  {
    public Task SendEmailAsync(SignUpRequest signUpRequest);
  }
}
