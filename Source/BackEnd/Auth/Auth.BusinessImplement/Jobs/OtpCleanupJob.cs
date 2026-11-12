using Auth.BusinessContract.Services;
using Auth.BusinessImplement.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Auth.BusinessImplement.Jobs
{
  // Scheduling only: decides when to run. What to delete is decided by IOtpService.CleanupExpiredAsync.
  // The hosted service is a singleton while IOtpService is scoped, so each run resolves it from a new scope.
  public class OtpCleanupJob(
    IServiceScopeFactory scopeFactory,
    IOptions<OtpOptions> options,
    TimeProvider timeProvider,
    ILogger<OtpCleanupJob> logger) : BackgroundService
  {
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      TimeSpan interval = TimeSpan.FromMinutes(options.Value.CleanupIntervalMinutes);

      // Runs once at startup, then on every tick. Runs never overlap because the loop is sequential.
      using PeriodicTimer timer = new(interval, timeProvider);
      do
      {
        await RunOnceAsync(stoppingToken);
      }
      while (await WaitForNextTickAsync(timer, stoppingToken));
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
      try
      {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        IOtpService otpService = scope.ServiceProvider.GetRequiredService<IOtpService>();
        await otpService.CleanupExpiredAsync(stoppingToken);
      }
      catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
      {
        // Shutting down
      }
      catch (Exception ex)
      {
        // Must not escape: an unhandled exception here would stop the whole host
        logger.LogError(ex, "OTP cleanup failed; it will be retried on the next run.");
      }
    }

    private static async Task<bool> WaitForNextTickAsync(PeriodicTimer timer, CancellationToken stoppingToken)
    {
      try
      {
        return await timer.WaitForNextTickAsync(stoppingToken);
      }
      catch (OperationCanceledException)
      {
        return false;
      }
    }
  }
}
