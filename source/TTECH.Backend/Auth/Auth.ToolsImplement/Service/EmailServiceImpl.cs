using Auth.BusinessContract.DTOs;
using Auth.ToolsContract.Service;
using Auth.ToolsImplement.Channels;

namespace Auth.ToolsImplement.Service
{
  public class EmailServiceImpl(EmailChannel channel) : IEmailService
  {
    public ValueTask SendEmailAsync(
      EmailMessage message,
      CancellationToken cancellationToken = default)
      => channel.Writer.WriteAsync(message, cancellationToken);
  }
}
