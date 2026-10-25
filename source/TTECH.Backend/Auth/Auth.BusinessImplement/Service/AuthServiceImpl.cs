using Auth.BusinessContract.DTO;
using Auth.BusinessContract.Service;
using Auth.BusinessImplement.Error;
using Auth.RepositoryContract.Entity;
using Common.DTO;
using Microsoft.AspNetCore.Identity;

namespace Auth.BusinessImplement.Service
{
  public class AuthServiceImpl(
    IUserService userService,
    ITokenService tokenService,
    UserManager<CoreUser> coreUserManager,
    SignInManager<CoreUser> signInManager) : IAuthService
  {
    private const bool IsLockOutOnFailure = false;

    public async Task<ServiceResult<TokensResponse>> SignUpAsync(SignUpRequest signUpRequest)
    {
      // 1. Validate request
      // Data Annotations

      // 2. Create user
      CoreUser? user = await userService.CreateUser(signUpRequest);
      if (user is null)
      {
        return ServiceResult.FromError<TokensResponse>(Errors.SignUpFailed);
      }

      // 3. Generate tokens
      TokensResponse? tokens = tokenService.GenerateTokenPair(user);
      if (tokens is null)
      {
        return ServiceResult.FromError<TokensResponse>(Errors.SignUpError);
      }

      return ServiceResult.FromResult(tokens);
    }

    public async Task<ServiceResult<TokensResponse>> SignInAsync(SignInRequest signInRequest)
    {
      // 1. Validate request
      // Data Annotations

      // 2. Check password
      CoreUser? user = await CheckPasswordAsync(signInRequest);
      if (user is null)
      {
        return ServiceResult.FromError<TokensResponse>(Errors.LogInFailed
          .WithMessage("Username or Password is incorrect. Please try again."));
      }

      // 3. Generate token
      TokensResponse? tokens = tokenService.GenerateTokenPair(user!);
      if (tokens is null)
      {
        return ServiceResult.FromError<TokensResponse>(Errors.LogInError);
      }

      return ServiceResult.FromResult(tokens!);
    }

    private async Task<CoreUser?> CheckPasswordAsync(SignInRequest signInRequest)
    {
      CoreUser? user = await coreUserManager.FindByNameAsync(signInRequest.Username);
      if (user is null) return null;

      SignInResult result = await signInManager.CheckPasswordSignInAsync(
        user,
        signInRequest.Password,
        IsLockOutOnFailure);
      if (result.Succeeded is false) return null;

      return user;
    }

    #region Verify OTP API
    public Task<ServiceResult<int>> SendOTPVerificationAsync(SendOTPVerificationRequest sendOTPVerificationRequest)
    {
      // 1. Validate request
      bool isEmailExists = userService.IsEmailExists(sendOTPVerificationRequest.Email);
      if (!isEmailExists)
      {
        return Task.FromResult(ServiceResult.FromError<int>(Errors.EmailNotFound));
      }

      // 2. Generate OTP
      int otp = GenerateOTP();
      userService.SaveOTP(sendOTPVerificationRequest.Email, otp);

      // 3. Send OTP to email
      userService.SendOTPEmail(sendOTPVerificationRequest.Email, otp);
      
      return Task.FromResult(ServiceResult.FromResult(1));
    }

    public Task<ServiceResult<TokensResponse>> VerifyOTPAsync(VerifyOTPRequest verifyOTPRequest)
    {
      // 1. Validate request
      bool isEmailExists = userService.IsEmailExists(verifyOTPRequest.Email);
      if (!isEmailExists)
      {
        return Task.FromResult(ServiceResult.FromError<bool>(Errors.EmailNotFound));
      }
      
      // 2. Verify OTP
      int? savedOTP = userService.GetSavedOTP(verifyOTPRequest.Email);
      if (savedOTP is null || savedOTP != int.Parse(verifyOTPRequest.OTP))
      {
        return Task.FromResult(ServiceResult.FromError<bool>(Errors.InvalidOTP));
      }

      // 3. Return authentication result  
            TokensResponse? tokens = tokenService.GenerateTokenPair(user!);
      if (tokens is null)
      {
        return ServiceResult.FromError<TokensResponse>(Errors.LogInError);
      }

      return ServiceResult.FromResult(tokens!);
    }
    #endregion
  }
}
