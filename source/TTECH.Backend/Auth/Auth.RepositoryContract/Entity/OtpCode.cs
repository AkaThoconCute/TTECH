namespace Auth.RepositoryContract.Entity
{
  public class OtpCode
  {
    public Guid Id { get; set; }

    // Upper-invariant, same as Identity's normalized email
    public string Email { get; set; } = string.Empty;
    public int Purpose { get; set; }

    // HMAC of the code; the plain code is never stored
    public string CodeHash { get; set; } = string.Empty;
    public int FailedAttempts { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
  }
}
