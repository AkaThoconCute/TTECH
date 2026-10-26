using Auth.BusinessContract.DTO;
using Auth.BusinessContract.Service;
using Auth.BusinessImplement.Email;

namespace Auth.BusinessImplement.Tools
{
  public class EmailServiceImpl(EmailQueue queue) : IEmailService
  {
    public ValueTask SendEmailAsync(
      EmailMessage message,
      CancellationToken cancellationToken = default)
      => queue.Writer.WriteAsync(message, cancellationToken);
  }
}
