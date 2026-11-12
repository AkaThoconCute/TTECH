using Auth.RepositoryContract.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Auth.RepositoryImplement.Context
{
  public class CoreDBContext(
    DbContextOptions<CoreDBContext> options,
    IConfiguration config) : IdentityDbContext<CoreUser>(options)
  {
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

      builder.Entity<OtpCode>(entity =>
      {
        entity.HasKey(o => o.Id);
        entity.Property(o => o.Email).HasMaxLength(256).IsRequired();
        entity.Property(o => o.CodeHash).HasMaxLength(128).IsRequired();
        entity.HasIndex(o => new { o.Email, o.Purpose });
        entity.HasIndex(o => o.ExpiresAt);
      });
    }

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

      // Migrations stay in the startup project, the DbContext lives in this assembly
      builder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly("Auth.AppHost"));
    }
  }
}
