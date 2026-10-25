using Auth.RepositoryImplement.Context;
using Auth.RepositoryContract.Entity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Auth.AppHost.Configuration
{
  public static class AuthConfigs
  {
    public static IServiceCollection AddAuthConfigs(
      this IServiceCollection services,
      IConfiguration config)
    {
      // 1. Add user config
      services
        .AddIdentity<CoreUser, IdentityRole>(options =>
        {
          options.Password.RequiredLength = 6;
          //options.Password.RequireDigit = true;
          //options.Password.RequireLowercase = false;
          //options.Password.RequireUppercase = false;
          //options.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<CoreDBContext>();

      // 2. Add token auth config
      services
        .AddAuthentication(options =>
        {
          options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
          options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
          options.TokenValidationParameters = new TokenValidationParameters
          {
            ValidIssuer = config["JWT:Issuer"],
            ValidateIssuer = true,
            ValidAudience = config["JWT:Audience"],
            ValidateAudience = true,
            IssuerSigningKey = new SymmetricSecurityKey(
              Encoding.UTF8.GetBytes(config["JWT:Key"]
              ?? throw new InvalidOperationException("JWT:Key was not found in configuration"))),
            ValidateIssuerSigningKey = true,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha512],
            ClockSkew = TimeSpan.Zero
          };
        });
      return services;
    }
  }
}
