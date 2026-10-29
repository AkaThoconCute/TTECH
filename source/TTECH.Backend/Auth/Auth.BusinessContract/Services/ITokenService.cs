using Auth.BusinessContract.DTOs;
using Auth.RepositoryContract.Entity;

namespace Auth.BusinessContract.Services
{
  public interface ITokenService
  {
    TokensResponse? GenerateTokenPair(CoreUser user);
  }
}
