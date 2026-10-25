using Auth.BusinessContract.DTO;
using Auth.BusinessImplement.Options;
using Common.Util;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Auth.BusinessImplement.Email
{
  // Delivers queued emails over SMTP. Recipient addresses and bodies are never logged.
  public class EmailDispatcher(
    EmailQueue queue,
    IOptions<EmailOptions> options,
    ILogger<EmailDispatcher> logger) : BackgroundService
  {
    private readonly EmailOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      await foreach (EmailMessage message in queue.Reader.ReadAllAsync(stoppingToken))
      {
        try
        {
          await RetryUtil.ExecuteAsync(
            ct => SendAsync(message, ct),
            _options.MaxRetries,
            EmailHelper.IsTransientFailure,
            (ex, attempt) => logger.LogWarning(ex, "Email delivery attempt {Attempt} failed; retrying.", attempt),
            cancellationToken: stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
          return;
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "Email delivery failed.");
        }
      }
    }

    private async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
      using SmtpClient client = new() { Timeout = _options.TimeoutSeconds * 1000 };
      await client.ConnectAsync(_options.Host, _options.Port, _options.SecureSocket, cancellationToken);

      if (!string.IsNullOrEmpty(_options.Username))
      {
        await client.AuthenticateAsync(_options.Username, _options.Password ?? string.Empty, cancellationToken);
      }

      await client.SendAsync(EmailHelper.BuildMimeMessage(_options, message), cancellationToken);
      await client.DisconnectAsync(true, cancellationToken);
    }
  }
}
