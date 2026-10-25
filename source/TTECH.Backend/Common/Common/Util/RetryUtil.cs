namespace Common.Util
{
  public static class RetryUtil
  {
    // Runs `operation` and retries it with exponential backoff (baseDelay, 2x, 4x, ...).
    // The last exception is rethrown when attempts run out, the error is not retryable, or the token is cancelled.
    public static async Task ExecuteAsync(
      Func<CancellationToken, Task> operation,
      int maxAttempts,
      Func<Exception, bool>? isRetryable = null,
      Action<Exception, int>? onRetry = null,
      TimeSpan? baseDelay = null,
      CancellationToken cancellationToken = default)
    {
      TimeSpan delay = baseDelay ?? TimeSpan.FromSeconds(2);

      for (int attempt = 1; ; attempt++)
      {
        try
        {
          await operation(cancellationToken);
          return;
        }
        catch (Exception ex) when (
          !cancellationToken.IsCancellationRequested
          && attempt < maxAttempts
          && (isRetryable?.Invoke(ex) ?? true))
        {
          onRetry?.Invoke(ex, attempt);
          await Task.Delay(delay * Math.Pow(2, attempt - 1), cancellationToken);
        }
      }
    }
  }
}
