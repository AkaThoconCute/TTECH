using Auth.BusinessContract.DTOs;
using Auth.ToolsContract.ServiceBus;

namespace Auth.ToolsImplement.Event
{
  public class EmailEvent(EmailQueue queue)
  {
    public ValueTask ProduceAsync(
      EmailMessage message,
      CancellationToken cancellationToken = default)
      => queue.Writer.WriteAsync(message, cancellationToken);
  }
}
