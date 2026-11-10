using Auth.BusinessContract.DTOs;
using Auth.ToolsContract.Services;
using Auth.ToolsImplement.Channels;

namespace Auth.ToolsImplement.Services
{
  public class EmailServiceImpl(EmailChannel channel) : IEmailService
  {
    public ValueTask SendEmailAsync(
      EmailMessage message,
      CancellationToken cancellationToken = default)
      => channel.Writer.WriteAsync(message, cancellationToken);
  }
}
