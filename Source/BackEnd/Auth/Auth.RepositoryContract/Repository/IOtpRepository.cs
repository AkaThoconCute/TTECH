using Auth.RepositoryContract.Entity;

namespace Auth.RepositoryContract.Repository
{
  public interface IOtpRepository
  {
    // At most one outstanding code exists per email + purpose.
    Task<OtpCode?> GetAsync(string email, int purpose, CancellationToken cancellationToken);

    // Replaces any outstanding code for the same email + purpose.
    Task ReplaceAsync(OtpCode otp, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    // Atomically bumps FailedAttempts and returns the new value (null if the code is gone or expired).
    Task<int?> RegisterAttemptAsync(Guid id, DateTime now, CancellationToken cancellationToken);

    // Atomically deletes the code. True for exactly one caller, which makes a code single-use.
    Task<bool> ConsumeAsync(Guid id, DateTime now, CancellationToken cancellationToken);

    Task<int> DeleteOlderThanAsync(DateTime threshold, CancellationToken cancellationToken);
  }
}
