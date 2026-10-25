using Auth.BusinessContract.DTO;
using Auth.BusinessContract.Enum;
using Auth.BusinessContract.Service;
using Auth.RepositoryContract.Entity;
using Common.DTO;

namespace Auth.BusinessImplement.Service
{
  public class EmailServiceImpl() : IEmailService
  {
    public async Task SendEmailAsync(SignUpRequest signUpRequest)
    {
      // Implement the logic to send an email here
      // For example, you can use an SMTP client or an email sending service

      await Task.CompletedTask;
    }
  }
}
