using Auth.RepositoryContract.Entity;
using Auth.RepositoryContract.Repository;
using Auth.RepositoryImplement.Context;
using Microsoft.EntityFrameworkCore;

namespace Auth.RepositoryImplement.Repository
{
  public class OtpRepository(CoreDBContext db) : IOtpRepository
  {
    public Task<OtpCode?> GetAsync(string email, int purpose, CancellationToken cancellationToken)
      => db.OtpCodes
        .AsNoTracking()
        .Where(o => o.Email == email && o.Purpose == purpose)
        .OrderByDescending(o => o.CreatedAt)
        .FirstOrDefaultAsync(cancellationToken);

    public async Task ReplaceAsync(OtpCode otp, CancellationToken cancellationToken)
    {
      await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

      await db.OtpCodes
        .Where(o => o.Email == otp.Email && o.Purpose == otp.Purpose)
        .ExecuteDeleteAsync(cancellationToken);

      db.OtpCodes.Add(otp);
      await db.SaveChangesAsync(cancellationToken);
      await transaction.CommitAsync(cancellationToken);
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
      => db.OtpCodes.Where(o => o.Id == id).ExecuteDeleteAsync(cancellationToken);

    public async Task<int?> RegisterAttemptAsync(Guid id, DateTime now, CancellationToken cancellationToken)
    {
      int updated = await db.OtpCodes
        .Where(o => o.Id == id && o.ExpiresAt > now)
        .ExecuteUpdateAsync(s => s.SetProperty(o => o.FailedAttempts, o => o.FailedAttempts + 1), cancellationToken);
      if (updated == 0) return null;

      return await db.OtpCodes
        .AsNoTracking()
        .Where(o => o.Id == id)
        .Select(o => (int?)o.FailedAttempts)
        .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ConsumeAsync(Guid id, DateTime now, CancellationToken cancellationToken)
    {
      int deleted = await db.OtpCodes
        .Where(o => o.Id == id && o.ExpiresAt > now)
        .ExecuteDeleteAsync(cancellationToken);
      return deleted == 1;
    }

    public Task<int> DeleteOlderThanAsync(DateTime threshold, CancellationToken cancellationToken)
      => db.OtpCodes
        .Where(o => o.ExpiresAt < threshold)
        .ExecuteDeleteAsync(cancellationToken);
  }
}
