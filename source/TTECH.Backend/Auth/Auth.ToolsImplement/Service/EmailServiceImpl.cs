using Auth.BusinessContract.DTO;
using Auth.ToolsContract.Service;

using Auth.ToolsImplement.Event;

namespace Auth.ToolsImplement.Service
{
  public class EmailServiceImpl(EmailEvent emailEvent) : IEmailService
  {
    public ValueTask SendEmailAsync(
      EmailMessage message,
      CancellationToken cancellationToken = default)
      => emailEvent.ProduceAsync(message, cancellationToken);
  }
}
