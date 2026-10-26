using Auth.BusinessContract.DTO;
using Auth.BusinessContract.Enum;
using Auth.BusinessContract.Service;
using Auth.BusinessImplement.Options;
using Auth.BusinessImplement.Service;
using Auth.RepositoryContract.Entity;
using Auth.RepositoryContract.Repository;
using Auth.ToolsContract.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Auth.Tests.UnitTests
{
  public partial class OtpServiceTests
  {
    private const string Email = "user@example.com";
    private const OtpPurposeEnum Purpose = OtpPurposeEnum.PasswordReset;

    private readonly FakeOtpRepository _repository = new();
    private readonly FakeEmailService _email = new();
    private readonly FakeTokenService _tokens = new();
    private readonly FakeTimeProvider _time = new();
    private readonly OtpServiceImpl _service;

    public OtpServiceTests()
    {
      OtpOptions options = new()
      {
        HashKey = "unit-test-hash-key-0123456789abcdef0123",
        ExpiryMinutes = 5,
        MaxAttempts = 3,
        ResendCooldownSeconds = 60
      };

      _service = new OtpServiceImpl(
        _repository,
        new FakeUserManager(new CoreUser { Id = "1", UserName = "user", FullName = "<b>Evil</b> User", Email = Email }),
        _tokens,
        _email,
        Options.Create(options),
        _time,
        NullLogger<OtpServiceImpl>.Instance);
    }

    private static SendOTPVerificationRequest SendRequest(string email = Email)
      => new() { Email = email, Purpose = Purpose };

    private static VerifyOTPRequest VerifyRequest(string code, string email = Email)
      => new() { Email = email, Purpose = Purpose, OTP = code };

    private async Task<string> SendAndGetCodeAsync()
    {
      await _service.SendAsync(SendRequest());
      return CodeRegex().Match(_email.Sent[^1].TextBody).Groups[1].Value;
    }

    private static string WrongCode(string code) => code == "000000" ? "000001" : "000000";

    #region Send
    [Fact]
    public async Task Send_UnknownEmail_ReturnsSameResponseAndSendsNothing()
    {
      var result = await _service.SendAsync(SendRequest("nobody@example.com"));

      Assert.True(result.Success);
      Assert.Equal(300, result.Result!.ExpiresInSeconds);
      Assert.Empty(_email.Sent);
      Assert.Empty(_repository.Rows);
    }

    [Fact]
    public async Task Send_KnownEmail_SendsSixDigitCodeAndStoresOnlyHash()
    {
      string code = await SendAndGetCodeAsync();

      Assert.Matches(@"^\d{6}$", code);
      Assert.DoesNotContain(code, _repository.Rows.Single().CodeHash);

      string flattened = Regex.Replace(_email.Sent.Single().HtmlBody, @"<[^>]+>|\s", "");
      Assert.Contains(code, flattened);
    }

    [Fact]
    public async Task Send_HtmlEncodesUserControlledValues()
    {
      await SendAndGetCodeAsync();

      string html = _email.Sent.Single().HtmlBody;
      Assert.DoesNotContain("<b>Evil</b>", html);
      Assert.Contains("&lt;b&gt;Evil&lt;/b&gt; User", html);
      Assert.DoesNotContain("{{", html);
    }

    [Fact]
    public async Task Send_WithinCooldown_DoesNotSendAgain()
    {
      await SendAndGetCodeAsync();
      await _service.SendAsync(SendRequest());
      Assert.Single(_email.Sent);

      _time.Advance(TimeSpan.FromSeconds(61));
      await _service.SendAsync(SendRequest());
      Assert.Equal(2, _email.Sent.Count);
      Assert.Single(_repository.Rows);
    }

    [Fact]
    public async Task Send_AfterTooManyFailedAttempts_IsLockedUntilCodeExpires()
    {
      string code = await SendAndGetCodeAsync();
      for (int i = 0; i < 3; i++)
      {
        await _service.VerifyAsync(VerifyRequest(WrongCode(code)));
      }

      _time.Advance(TimeSpan.FromSeconds(61));
      await _service.SendAsync(SendRequest());
      Assert.Single(_email.Sent);

      _time.Advance(TimeSpan.FromMinutes(5));
      await _service.SendAsync(SendRequest());
      Assert.Equal(2, _email.Sent.Count);
    }

    [Fact]
    public async Task CleanupExpired_DeletesOnlyCodesExpiredMoreThanADayAgo()
    {
      DateTime now = _time.GetUtcNow().UtcDateTime;
      OtpCode Row(string email, TimeSpan expiredFor) => new()
      {
        Id = Guid.NewGuid(),
        Email = email,
        Purpose = (int)Purpose,
        CodeHash = "x",
        CreatedAt = now - expiredFor - TimeSpan.FromMinutes(5),
        ExpiresAt = now - expiredFor
      };
      _repository.Rows.Add(Row("OLD@EXAMPLE.COM", TimeSpan.FromDays(2)));
      _repository.Rows.Add(Row("RECENT@EXAMPLE.COM", TimeSpan.FromHours(1)));
      _repository.Rows.Add(Row("ACTIVE@EXAMPLE.COM", TimeSpan.FromMinutes(-3)));

      await _service.CleanupExpiredAsync();

      Assert.Equal(["RECENT@EXAMPLE.COM", "ACTIVE@EXAMPLE.COM"], _repository.Rows.Select(r => r.Email));
    }

    [Fact]
    public async Task Send_DoesNotRunCleanup()
    {
      DateTime now = _time.GetUtcNow().UtcDateTime;
      _repository.Rows.Add(new OtpCode
      {
        Id = Guid.NewGuid(),
        Email = "OLD@EXAMPLE.COM",
        Purpose = (int)Purpose,
        CodeHash = "x",
        CreatedAt = now.AddDays(-3),
        ExpiresAt = now.AddDays(-3).AddMinutes(5)
      });

      await SendAndGetCodeAsync();

      Assert.Equal(2, _repository.Rows.Count);
    }

    [Fact]
    public async Task Send_UndefinedPurpose_Throws()
    {
      await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
        () => _service.SendAsync(new SendOTPVerificationRequest { Email = Email, Purpose = (OtpPurposeEnum)99 }));
    }
    #endregion

    #region Verify
    [Fact]
    public async Task Verify_CorrectCode_ReturnsSignInTokensOnlyOnce()
    {
      string code = await SendAndGetCodeAsync();

      var first = await _service.VerifyAsync(VerifyRequest(code));
      Assert.True(first.Success);
      Assert.Equal("access", first.Result!.AccessToken);
      Assert.Equal("1", _tokens.LastUserId);
      Assert.Empty(_repository.Rows);

      var second = await _service.VerifyAsync(VerifyRequest(code));
      Assert.False(second.Success);
    }

    [Fact]
    public async Task Verify_EmailIsCaseInsensitive()
    {
      string code = await SendAndGetCodeAsync();

      var result = await _service.VerifyAsync(VerifyRequest(code, Email.ToUpperInvariant()));
      Assert.True(result.Success);
    }

    [Fact]
    public async Task Verify_WrongCode_Fails()
    {
      string code = await SendAndGetCodeAsync();

      var result = await _service.VerifyAsync(VerifyRequest(WrongCode(code)));

      Assert.False(result.Success);
      Assert.Equal(2003, result.Error!.Code);
      Assert.Null(_tokens.LastUserId);
    }

    [Fact]
    public async Task Verify_AfterMaxAttempts_RejectsEvenCorrectCode()
    {
      string code = await SendAndGetCodeAsync();
      for (int i = 0; i < 3; i++)
      {
        await _service.VerifyAsync(VerifyRequest(WrongCode(code)));
      }

      var result = await _service.VerifyAsync(VerifyRequest(code));
      Assert.False(result.Success);
    }

    [Fact]
    public async Task Verify_Expired_Fails()
    {
      string code = await SendAndGetCodeAsync();
      _time.Advance(TimeSpan.FromMinutes(5));

      var result = await _service.VerifyAsync(VerifyRequest(code));
      Assert.False(result.Success);
    }

    [Fact]
    public async Task Verify_WithoutSend_Fails()
    {
      var result = await _service.VerifyAsync(VerifyRequest("123456"));
      Assert.False(result.Success);
    }

    [Fact]
    public async Task Verify_WhenTokenGenerationFails_ReturnsServerError()
    {
      string code = await SendAndGetCodeAsync();
      _tokens.Fail = true;

      var result = await _service.VerifyAsync(VerifyRequest(code));

      Assert.False(result.Success);
      Assert.Equal(2002, result.Error!.Code);
    }
    #endregion

    [GeneratedRegex(@": (\d{6})\n")]
    private static partial Regex CodeRegex();

    #region Fakes
    private sealed class FakeTimeProvider : TimeProvider
    {
      private DateTimeOffset _now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
      public override DateTimeOffset GetUtcNow() => _now;
      public void Advance(TimeSpan by) => _now += by;
    }

    private sealed class FakeEmailService : IEmailService
    {
      public List<EmailMessage> Sent { get; } = [];

      public ValueTask SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default)
      {
        Sent.Add(message);
        return ValueTask.CompletedTask;
      }
    }

    private sealed class FakeTokenService : ITokenService
    {
      public bool Fail { get; set; }
      public string? LastUserId { get; private set; }

      public TokensResponse? GenerateTokenPair(CoreUser user)
      {
        if (Fail) return null;
        LastUserId = user.Id;
        return new TokensResponse { AccessToken = "access", RefreshToken = "refresh" };
      }
    }

    private sealed class FakeUserManager(CoreUser user)
      : UserManager<CoreUser>(DispatchProxy.Create<IUserStore<CoreUser>, StoreProxy>(), null!, null!, null!, null!, null!, null!, null!, NullLogger<UserManager<CoreUser>>.Instance)
    {
      public override Task<CoreUser?> FindByEmailAsync(string email)
        => Task.FromResult<CoreUser?>(
          string.Equals(email, user.Email, StringComparison.OrdinalIgnoreCase) ? user : null);
    }

    public class StoreProxy : DispatchProxy
    {
      protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        => throw new NotSupportedException();
    }

    public sealed class FakeOtpRepository : IOtpRepository
    {
      public List<OtpCode> Rows { get; } = [];

      public Task<OtpCode?> GetAsync(string email, int purpose, CancellationToken ct)
        => Task.FromResult(Rows
          .Where(r => r.Email == email && r.Purpose == purpose)
          .OrderByDescending(r => r.CreatedAt)
          .Select(Clone)
          .FirstOrDefault());

      public Task ReplaceAsync(OtpCode otp, CancellationToken ct)
      {
        Rows.RemoveAll(r => r.Email == otp.Email && r.Purpose == otp.Purpose);
        Rows.Add(Clone(otp));
        return Task.CompletedTask;
      }

      public Task DeleteAsync(Guid id, CancellationToken ct)
      {
        Rows.RemoveAll(r => r.Id == id);
        return Task.CompletedTask;
      }

      public Task<int?> RegisterAttemptAsync(Guid id, DateTime now, CancellationToken ct)
      {
        OtpCode? row = Rows.FirstOrDefault(r => r.Id == id && r.ExpiresAt > now);
        if (row is null) return Task.FromResult<int?>(null);
        row.FailedAttempts++;
        return Task.FromResult<int?>(row.FailedAttempts);
      }

      public Task<bool> ConsumeAsync(Guid id, DateTime now, CancellationToken ct)
        => Task.FromResult(Rows.RemoveAll(r => r.Id == id && r.ExpiresAt > now) == 1);

      public Task<int> DeleteOlderThanAsync(DateTime threshold, CancellationToken ct)
        => Task.FromResult(Rows.RemoveAll(r => r.ExpiresAt < threshold));

      private static OtpCode Clone(OtpCode r) => new()
      {
        Id = r.Id, Email = r.Email, Purpose = r.Purpose, CodeHash = r.CodeHash,
        FailedAttempts = r.FailedAttempts, CreatedAt = r.CreatedAt, ExpiresAt = r.ExpiresAt
      };
    }
    #endregion
  }
}
