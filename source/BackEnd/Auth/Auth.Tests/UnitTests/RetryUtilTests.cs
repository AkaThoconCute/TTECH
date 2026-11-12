using Common.Util;

namespace Auth.Tests.UnitTests
{
  public class RetryUtilTests
  {
    [Fact]
    public async Task Succeeds_AfterTransientFailures()
    {
      int calls = 0;
      int retries = 0;

      await RetryUtil.ExecuteAsync(
        _ => ++calls < 3 ? throw new InvalidOperationException() : Task.CompletedTask,
        maxAttempts: 3,
        onRetry: (_, _) => retries++,
        baseDelay: TimeSpan.Zero);

      Assert.Equal(3, calls);
      Assert.Equal(2, retries);
    }

    [Fact]
    public async Task Rethrows_WhenAttemptsRunOut()
    {
      int calls = 0;

      await Assert.ThrowsAsync<InvalidOperationException>(() => RetryUtil.ExecuteAsync(
        _ => { calls++; throw new InvalidOperationException(); },
        maxAttempts: 3,
        baseDelay: TimeSpan.Zero));

      Assert.Equal(3, calls);
    }

    [Fact]
    public async Task DoesNotRetry_WhenErrorIsNotRetryable()
    {
      int calls = 0;

      await Assert.ThrowsAsync<ArgumentException>(() => RetryUtil.ExecuteAsync(
        _ => { calls++; throw new ArgumentException(); },
        maxAttempts: 5,
        isRetryable: ex => ex is not ArgumentException,
        baseDelay: TimeSpan.Zero));

      Assert.Equal(1, calls);
    }

    [Fact]
    public async Task StopsRetrying_WhenCancelled()
    {
      using CancellationTokenSource cts = new();
      int calls = 0;

      await Assert.ThrowsAnyAsync<Exception>(() => RetryUtil.ExecuteAsync(
        _ => { calls++; cts.Cancel(); throw new InvalidOperationException(); },
        maxAttempts: 5,
        baseDelay: TimeSpan.Zero,
        cancellationToken: cts.Token));

      Assert.Equal(1, calls);
    }
  }
}
