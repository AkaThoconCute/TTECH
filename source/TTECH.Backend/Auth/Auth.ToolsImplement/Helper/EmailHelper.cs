using Auth.BusinessContract.DTO;
using Auth.ToolsImplement.Options;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Utils;
using System.Collections.Concurrent;
using System.Net;
using System.Text.RegularExpressions;

namespace Auth.ToolsImplement.Helper
{
  // Low-level email tools: MIME building, template rendering, SMTP failure classification.
  public static partial class EmailHelper
  {
    private static readonly ConcurrentDictionary<string, string> TemplateCache = new();

    public static MimeMessage BuildMimeMessage(EmailOptions options, EmailMessage message)
    {
      MimeMessage mime = new();
      mime.From.Add(new MailboxAddress(options.FromName, options.FromAddress));
      mime.To.Add(new MailboxAddress(message.ToName ?? string.Empty, message.ToAddress));
      mime.Subject = message.Subject;
      mime.MessageId = MimeUtils.GenerateMessageId(options.FromAddress[(options.FromAddress.IndexOf('@') + 1)..]);

      // Tell auto-responders / out-of-office systems not to reply to transactional mail
      mime.Headers.Add("Auto-Submitted", "auto-generated");
      mime.Headers.Add("X-Auto-Response-Suppress", "All");

      // multipart/alternative: plain text + HTML
      mime.Body = new BodyBuilder
      {
        TextBody = message.TextBody,
        HtmlBody = message.HtmlBody
      }.ToMessageBody();

      return mime;
    }

    // Replaces {{key}} placeholders. Values are HTML-encoded because some come from user input.
    public static string RenderTemplate(string templateFileName, IReadOnlyDictionary<string, string> values)
    {
      string template = TemplateCache.GetOrAdd(templateFileName, LoadTemplate);

      return PlaceholderRegex().Replace(
        template,
        m => values.TryGetValue(m.Groups[1].Value, out string? value) ? WebUtility.HtmlEncode(value) : m.Value);
    }

    // 5xx SMTP replies are permanent (bad mailbox, rejected sender, ...); retrying cannot help
    public static bool IsTransientFailure(Exception ex)
      => ex is not SmtpCommandException { StatusCode: >= (SmtpStatusCode)500 };

    private static string LoadTemplate(string templateFileName)
      => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Resources", "Templates", templateFileName));

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex PlaceholderRegex();
  }
}
