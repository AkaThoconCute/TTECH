using Auth.BusinessContract.Services;
using Auth.BusinessImplement.Jobs;
using Auth.BusinessImplement.Options;
using Auth.BusinessImplement.Services;
using Auth.ToolsContract.Service;
using Auth.ToolsImplement.Channels;
using Auth.ToolsImplement.Options;
using Auth.ToolsImplement.Service;
using Auth.ToolsImplement.Worker;
using Auth.RepositoryContract.Repository;
using Auth.RepositoryImplement.Repository;

namespace Auth.AppHost.DependencyInjection
{
  public static class DependencyInjection
  {
    public static IServiceCollection AddDependencyInjection(
      this IServiceCollection services,
      IConfiguration config)
    {
      services.AddScoped<IAuthService, AuthServiceImpl>();
      services.AddScoped<IUserService, UserServiceImpl>();
      services.AddScoped<ITokenService, TokenServiceImpl>();

      // OTP
      services
        .AddOptions<OtpOptions>()
        .Bind(config.GetSection(OtpOptions.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();
      services.AddSingleton(TimeProvider.System);
      services.AddScoped<IOtpRepository, OtpRepository>();
      services.AddScoped<IOtpService, OtpServiceImpl>();
      services.AddHostedService<OtpCleanupJob>();

      // Email (queued, delivered in the background)
      services
        .AddOptions<EmailOptions>()
        .Bind(config.GetSection(EmailOptions.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();
      services.AddSingleton<EmailChannel>();
      services.AddSingleton<IEmailService, EmailServiceImpl>();
      services.AddHostedService<EmailWorker>();

      return services;
    }
  }
}
