using Auth.BusinessContract.DTOs;
using Auth.BusinessContract.Enums;
using Auth.BusinessContract.Services;
using Auth.ToolsContract.Service;
using Auth.BusinessImplement.Errors;
using Auth.BusinessImplement.Helpers;
using Auth.BusinessImplement.Options;
using Auth.RepositoryContract.Entity;
using Auth.RepositoryContract.Repository;
using Auth.ToolsImplement.Helper;
using Common.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Auth.BusinessImplement.Services
{
  public class OtpServiceImpl(
    IOtpRepository otpRepository,
    UserManager<CoreUser> userManager,
    ITokenService tokenService,
    IEmailService emailService,
    IOptions<OtpOptions> options,
    TimeProvider timeProvider,
    ILogger<OtpServiceImpl> logger) : IOtpService
  {
    private readonly OtpOptions _options = options.Value;

    // What each purpose says to the user and which template it uses
    private sealed record PurposeContent(string Action, string Subject, string TemplateFileName);

    #region Send OTP API
    public async Task<ServiceResult<SendOTPVerificationResponse>> SendAsync(
      SendOTPVerificationRequest sendOTPVerificationRequest,
      CancellationToken cancellationToken = default)
    {
      // 1. Validate request
      OtpPurposeEnum purpose = sendOTPVerificationRequest.Purpose;
      EnsureValidPurpose(purpose);

      // Same response whether or not the account exists, so the endpoint cannot be used to enumerate emails
      ServiceResult<SendOTPVerificationResponse> accepted = ServiceResult.FromResult(new SendOTPVerificationResponse
      {
        ExpiresInSeconds = _options.ExpiryMinutes * 60,
        ResendAfterSeconds = _options.ResendCooldownSeconds
      });

      CoreUser? user = await userManager.FindByEmailAsync(sendOTPVerificationRequest.Email);
      if (user?.Email is null) return accepted;

      string normalizedEmail = OtpHelper.NormalizeEmail(sendOTPVerificationRequest.Email);
      DateTime now = timeProvider.GetUtcNow().UtcDateTime;

      // 2. Throttle: ignore silently so the endpoint can't be used to spam a mailbox or to farm guesses
      OtpCode? latest = await otpRepository.GetAsync(normalizedEmail, (int)purpose, cancellationToken);
      if (latest is not null && IsThrottled(latest, now)) return accepted;

      // 3. Generate and store the code (hash only)
      string code = OtpHelper.GenerateCode();
      OtpCode otp = new()
      {
        Id = Guid.NewGuid(),
        Email = normalizedEmail,
        Purpose = (int)purpose,
        CodeHash = OtpHelper.ComputeHash(_options.HashKey, purpose, normalizedEmail, code),
        CreatedAt = now,
        ExpiresAt = now.AddMinutes(_options.ExpiryMinutes)
      };
      await otpRepository.ReplaceAsync(otp, cancellationToken);

      // 4. Send the code by email
      try
      {
        await emailService.SendEmailAsync(BuildEmail(user, purpose, code), cancellationToken);
      }
      catch
      {
        // Do not leave a code the user can never receive
        await otpRepository.DeleteAsync(otp.Id, CancellationToken.None);
        throw;
      }

      logger.LogInformation("OTP issued for purpose {Purpose}, user {UserId}.", purpose, user.Id);
      return accepted;
    }
    #endregion

    #region Verify OTP API
    public async Task<ServiceResult<TokensResponse>> VerifyAsync(
      VerifyOTPRequest verifyOTPRequest,
      CancellationToken cancellationToken = default)
    {
      // 1. Validate request
      OtpPurposeEnum purpose = verifyOTPRequest.Purpose;
      EnsureValidPurpose(purpose);

      ServiceResult<TokensResponse> invalid = ServiceResult.FromError<TokensResponse>(Errors.Errors.InvalidOTP);

      string normalizedEmail = OtpHelper.NormalizeEmail(verifyOTPRequest.Email);
      DateTime now = timeProvider.GetUtcNow().UtcDateTime;

      OtpCode? otp = await otpRepository.GetAsync(normalizedEmail, (int)purpose, cancellationToken);
      if (otp is null || otp.ExpiresAt <= now) return invalid;

      // 2. Count the attempt first (atomic) so concurrent guesses cannot exceed the limit
      int? attempts = await otpRepository.RegisterAttemptAsync(otp.Id, now, cancellationToken);
      if (attempts is null || attempts > _options.MaxAttempts) return invalid;

      string expectedHash = OtpHelper.ComputeHash(_options.HashKey, purpose, normalizedEmail, verifyOTPRequest.OTP);
      if (!OtpHelper.HashEquals(expectedHash, otp.CodeHash)) return invalid;

      // 3. Single-use: only one caller can consume the code
      if (!await otpRepository.ConsumeAsync(otp.Id, now, cancellationToken)) return invalid;

      // 4. Authenticate, same result as sign in
      CoreUser? user = await userManager.FindByEmailAsync(verifyOTPRequest.Email);
      if (user is null) return invalid;

      TokensResponse? tokens = tokenService.GenerateTokenPair(user);
      if (tokens is null)
      {
        return ServiceResult.FromError<TokensResponse>(Errors.Errors.LogInError);
      }

      return ServiceResult.FromResult(tokens);
    }
    #endregion

    #region Cleanup OTP (the schedule lives in OtpCleanupJob)
    public Task CleanupExpiredAsync(CancellationToken cancellationToken = default)
      => otpRepository.DeleteOlderThanAsync(
        timeProvider.GetUtcNow().UtcDateTime - TimeSpan.FromHours(_options.CleanupRetentionHours),
        cancellationToken);
    #endregion

    #region Private methods
    private static void EnsureValidPurpose(OtpPurposeEnum purpose)
    {
      if (!Enum.IsDefined(purpose))
      {
        throw new ArgumentOutOfRangeException(nameof(purpose), purpose, "Unknown OTP purpose.");
      }
    }

    // Resend cooldown, or locked after too many failed guesses until that code expires
    private bool IsThrottled(OtpCode latest, DateTime now)
    {
      bool inCooldown = now - latest.CreatedAt < TimeSpan.FromSeconds(_options.ResendCooldownSeconds);
      bool isLocked = latest.FailedAttempts >= _options.MaxAttempts && latest.ExpiresAt > now;
      return inCooldown || isLocked;
    }

    private static PurposeContent DescribePurpose(OtpPurposeEnum purpose) => purpose switch
    {
      OtpPurposeEnum.PasswordReset => new(
        "reset your password",
        "Your password reset code",
        "PasswordResetTemplate.html"),
      _ => throw new ArgumentOutOfRangeException(nameof(purpose), purpose, "Unknown OTP purpose.")
    };

    private EmailMessage BuildEmail(CoreUser user, OtpPurposeEnum purpose, string code)
    {
      PurposeContent content = DescribePurpose(purpose);
      string clientName = string.IsNullOrWhiteSpace(user.FullName) ? user.UserName ?? user.Email! : user.FullName;

      Dictionary<string, string> values = new()
      {
        ["system_name"] = _options.SystemName,
        ["client_name"] = clientName,
        ["verification_action"] = content.Action,
        ["otp_expiry_minutes"] = _options.ExpiryMinutes.ToString(),
        ["support_url"] = _options.SupportUrl
      };
      for (int i = 0; i < OtpHelper.CodeLength; i++)
      {
        values[(i + 1).ToString()] = code[i].ToString();
      }

      string text =
        $"Hello {clientName},\n\n" +
        $"Use this code to {content.Action}: {code}\n\n" +
        $"The code is valid for {_options.ExpiryMinutes} minutes and can only be used once.\n" +
        "If you did not request this code, please ignore this email. Never share your code with anyone.\n\n" +
        $"The {_options.SystemName} Team";

      return new EmailMessage
      {
        ToAddress = user.Email!,
        ToName = clientName,
        Subject = $"{content.Subject} - {_options.SystemName}",
        HtmlBody = EmailHelper.RenderTemplate(content.TemplateFileName, values),
        TextBody = text
      };
    }
    #endregion
  }
}
