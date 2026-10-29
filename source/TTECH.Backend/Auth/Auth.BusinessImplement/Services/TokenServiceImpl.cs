using Auth.BusinessContract.DTOs;
using Auth.BusinessContract.Services;
using Auth.RepositoryContract.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Auth.BusinessImplement.Services
{
  public class TokenServiceImpl(
    UserManager<CoreUser> userManager,
    IConfiguration config) : ITokenService

  {
    // GenerateTokenPair
    public TokensResponse? GenerateTokenPair(CoreUser user)
    {
      string? accessToken = GenerateAccessToken(user);
      if (accessToken is null) return null;

      string? refreshToken = GenerateRefreshToken(user);
      if (refreshToken is null) return null;

      return new TokensResponse()
      {
        AccessToken = accessToken!,
        RefreshToken = refreshToken!
      };
    }

    // GenerateAccessToken
    private string? GenerateAccessToken(CoreUser user) //, IList<string> roles)
    {
      try
      {
        // 1. Get Token config
        if (user is null) return null;

        string? key = config.GetValue<string?>("JWT:Key");
        if (string.IsNullOrEmpty(key)) return null;

        // Check key is 512 bits (64 bytes) for HS512
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        if (keyBytes.Length < 64) return null;

        SymmetricSecurityKey securityKey = new(keyBytes);
        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha512Signature);

        double duration = config.GetValue<double>("JWT:Duration");
        string? issuer = config["JWT:Issuer"];
        string? audience = config["JWT:Audience"];

        // 2. Tạo danh sách Claims
        List<Claim> claims =
        [
          new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
          new(JwtRegisteredClaimNames.GivenName, user.UserName ?? ""),
          new(JwtRegisteredClaimNames.NameId, user.Id)
        ];

        //foreach (string role in roles)
        //{
        //  claims.Add(new Claim(ClaimTypes.Role, role));
        //}

        // 3. Create token
        SecurityTokenDescriptor tokenDescriptor = new()
        {
          Subject = new ClaimsIdentity(claims),
          Expires = DateTime.UtcNow.AddDays(duration),
          SigningCredentials = credentials,
          Issuer = issuer,
          Audience = audience
        };

        JwtSecurityTokenHandler tokenHandler = new();
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        string tokenString = tokenHandler.WriteToken(token);

        return tokenString;
      }
      catch (Exception)
      {
        return null;
      }
    }

    // GenerateRefreshToken
    private string? GenerateRefreshToken(CoreUser user)
    {
      try
      {
        if (user is null) return null;

        byte[] randomNumber = new byte[64];
        using var randomGenerator = RandomNumberGenerator.Create();
        randomGenerator.GetBytes(randomNumber);
        string tokenString = Convert.ToBase64String(randomNumber);

        double duration = config.GetValue<double>("JWT:Duration");

        user.RefreshToken = tokenString;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(duration);
        userManager.UpdateAsync(user);

        return tokenString;
      }
      catch (Exception)
      {
        return null;
      }
    }
  }
}
