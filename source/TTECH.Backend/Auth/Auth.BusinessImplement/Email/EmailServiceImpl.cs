using Auth.BusinessContract.DTO;
using Auth.BusinessContract.Service;

namespace Auth.BusinessImplement.Email
{
  public class EmailServiceImpl(EmailQueue queue) : IEmailService
  {
    public ValueTask SendEmailAsync(
      EmailMessage message,
      CancellationToken cancellationToken = default)
      => queue.Writer.WriteAsync(message, cancellationToken);
  }
}
