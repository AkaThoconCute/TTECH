using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Web.HealthCheck
{
  public static class DatabaseHealthCheck
  {
    public static async Task EnsureDatabaseHealthy<TDbContext>(this IHost host)
      where TDbContext : DbContext
    {
      using IServiceScope scope = host.Services.CreateScope();
      IServiceProvider services = scope.ServiceProvider;

      TDbContext databaseContext = services.GetRequiredService<TDbContext>();
      if (await databaseContext.Database.CanConnectAsync())
        return;

      ILogger<TDbContext> logger = services.GetRequiredService<ILogger<TDbContext>>();
      string databaseName = typeof(TDbContext).Name;
      string errorMessage = $">>> Database [{databaseName}] connection failed";

      logger.LogCritical("{ErrorMessage}", errorMessage);
      throw new InvalidOperationException(errorMessage);
    }
  }
}
