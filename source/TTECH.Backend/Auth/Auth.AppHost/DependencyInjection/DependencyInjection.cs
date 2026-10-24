using Auth.BusinessContract.Service;
using Auth.BusinessImplement.Service;

namespace Auth.AppHost.DependencyInjection
{
  public static class DependencyInjection
  {
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
    {
      services.AddScoped<IAuthService, AuthServiceImpl>();
      services.AddScoped<IUserService, UserServiceImpl>();
      services.AddScoped<ITokenService, TokenServiceImpl>();

      return services;
    }
  }
}
