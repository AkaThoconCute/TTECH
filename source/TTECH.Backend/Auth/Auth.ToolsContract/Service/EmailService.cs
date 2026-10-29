using Auth.BusinessContract.DTOs;

namespace Auth.ToolsContract.Service
{
  public interface IEmailService
  {
    // Queues the message for background delivery so request latency does not depend on the SMTP server.
    public ValueTask SendEmailAsync(
      EmailMessage message,
      CancellationToken cancellationToken = default);
  }
}
