using Microsoft.AspNetCore.Identity;

namespace Auth.RepositoryContract.Entity
{
  public class CoreUser : IdentityUser
  {
    public string FullName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
  }
}
