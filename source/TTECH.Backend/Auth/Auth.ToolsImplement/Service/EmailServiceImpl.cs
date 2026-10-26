using Auth.BusinessContract.DTO;
using Auth.BusinessContract.Service;
using Auth.BusinessImplement.Email;
using Auth.ToolsImplement.Event;

namespace Auth.BusinessImplement.Tools
{
  public class EmailServiceImpl(EmailEvent emailEvent) : IEmailService
  {
    public ValueTask SendEmailAsync(
      EmailMessage message,
      CancellationToken cancellationToken = default)
      => emailEvent.ProduceAsync(message, cancellationToken);
  }
}
