using Auth.BusinessImplement.Email;
using System.Net.Mail;
using System.Text.Json;

namespace Auth.ToolsImplement.Event
{
  public class EmailEvent(EmailQueue queue)
  {
    public async Task ProduceAsync(
      EmailMessage message,
      CancellationToken cancellationToken = default)
    {
      await queue.Writer.WriteAsync(message, cancellationToken);
    }
  }
}
