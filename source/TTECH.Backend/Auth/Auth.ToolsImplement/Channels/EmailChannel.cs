using Auth.BusinessContract.DTOs;
using System.Threading.Channels;

namespace Auth.ToolsImplement.Channels
{
  // Singleton in-memory channel shared by the producer (EmailServiceImpl) and the consumer (EmailWorker).
  public class EmailChannel
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
