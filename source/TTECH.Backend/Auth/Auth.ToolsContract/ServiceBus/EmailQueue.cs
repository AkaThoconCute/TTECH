using Auth.BusinessContract.DTOs;
using System.Threading.Channels;

namespace Auth.ToolsContract.ServiceBus
{
  // Singleton in-memory queue shared by the producer (EmailServiceImpl) and the consumer (SendEmailFunction).
  public class EmailQueue
  {
    private readonly Channel<EmailMessage> _channel = Channel.CreateBounded<EmailMessage>(
      new BoundedChannelOptions(1000)
      {
        SingleReader = true,
        FullMode = BoundedChannelFullMode.Wait
      });

    public ChannelWriter<EmailMessage> Writer => _channel.Writer;
    public ChannelReader<EmailMessage> Reader => _channel.Reader;
  }
}
