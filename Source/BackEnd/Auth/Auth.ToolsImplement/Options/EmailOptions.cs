using MailKit.Security;
using System.ComponentModel.DataAnnotations;

namespace Auth.ToolsImplement.Options
{
  public class EmailOptions
  {
    public const string SectionName = "Email";

    [Required]
    public string Host { get; init; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; init; } = 587;

    // StartTls for 587, SslOnConnect for 465, None only for local catch-all servers (Mailpit/smtp4dev)
    public SecureSocketOptions SecureSocket { get; init; } = SecureSocketOptions.StartTls;

    // Leave empty for servers without authentication. Supply real values via user-secrets / env vars.
    public string? Username { get; init; }
    public string? Password { get; init; }

    [Required, EmailAddress]
    public string FromAddress { get; init; } = string.Empty;

    [Required]
    public string FromName { get; init; } = string.Empty;

    [Range(1, 300)]
    public int TimeoutSeconds { get; init; } = 30;

    [Range(1, 10)]
    public int MaxRetries { get; init; } = 3;
  }
}
