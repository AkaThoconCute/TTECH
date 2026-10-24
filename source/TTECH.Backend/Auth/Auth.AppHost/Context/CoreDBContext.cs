using Auth.RepositoryContract.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.AppHost.Context
{
  public class CoreDBContext(
    DbContextOptions<CoreDBContext> options,
    IConfiguration config) : IdentityDbContext<CoreUser>(options)
  {
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
      if (builder.IsConfigured)
      {
        throw new InvalidOperationException(
          "ConnectionStrings.Database must not be pre-configured before CoreDBContext.");
      }

      string? connectionString = config.GetConnectionString("Database");
      if (string.IsNullOrEmpty(connectionString))
      {
        throw new InvalidOperationException(
          "ConnectionStrings.Database was not found in configuration.");
      }

      builder.UseSqlServer(connectionString);
    }
  }
}
