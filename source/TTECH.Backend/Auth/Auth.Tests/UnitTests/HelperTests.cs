using Auth.BusinessContract.Enum;
using Auth.BusinessImplement.Email;
using Auth.BusinessImplement.Helper;
using MailKit.Net.Smtp;

namespace Auth.Tests.UnitTests
{
  public class HelperTests
  {
    private const string Key = "unit-test-hash-key-0123456789abcdef0123";

    [Fact]
    public void GenerateCode_ReturnsSixDigits()
    {
      for (int i = 0; i < 200; i++)
      {
        Assert.Matches(@"^\d{6}$", OtpHelper.GenerateCode());
      }
    }

    [Fact]
    public void NormalizeEmail_TrimsAndUppercases()
      => Assert.Equal("A@B.COM", OtpHelper.NormalizeEmail("  a@b.com "));

    [Fact]
    public void ComputeHash_DependsOnEmailKeyAndCode()
    {
      string baseline = OtpHelper.ComputeHash(Key, OtpPurposeEnum.PasswordReset, "A@B.COM", "123456");

      Assert.Equal(baseline, OtpHelper.ComputeHash(Key, OtpPurposeEnum.PasswordReset, "A@B.COM", "123456"));
      Assert.NotEqual(baseline, OtpHelper.ComputeHash(Key, OtpPurposeEnum.PasswordReset, "C@B.COM", "123456"));
      Assert.NotEqual(baseline, OtpHelper.ComputeHash(Key, OtpPurposeEnum.PasswordReset, "A@B.COM", "123457"));
      Assert.NotEqual(baseline, OtpHelper.ComputeHash(Key + "x", OtpPurposeEnum.PasswordReset, "A@B.COM", "123456"));
    }

    [Fact]
    public void HashEquals_ComparesValues()
    {
      Assert.True(OtpHelper.HashEquals("abc", "abc"));
      Assert.False(OtpHelper.HashEquals("abc", "abd"));
      Assert.False(OtpHelper.HashEquals("abc", "abcd"));
    }

    [Fact]
    public void IsTransientFailure_TreatsOnly5xxSmtpRepliesAsPermanent()
    {
      Assert.True(EmailHelper.IsTransientFailure(new IOException()));
      Assert.True(EmailHelper.IsTransientFailure(
        new SmtpCommandException(SmtpErrorCode.UnexpectedStatusCode, SmtpStatusCode.ServiceNotAvailable, "busy")));
      Assert.False(EmailHelper.IsTransientFailure(
        new SmtpCommandException(SmtpErrorCode.RecipientNotAccepted, SmtpStatusCode.MailboxUnavailable, "no such user")));
    }

    [Fact]
    public void RenderTemplate_ReplacesKnownPlaceholdersAndEncodesValues()
    {
      string html = EmailHelper.RenderTemplate(
        "PasswordResetTemplate.html",
        new Dictionary<string, string> { ["client_name"] = "<script>", ["system_name"] = "TTECH" });

      Assert.Contains("&lt;script&gt;", html);
      Assert.DoesNotContain("{{client_name}}", html);
      Assert.Contains("{{support_url}}", html);
    }
  }
}
