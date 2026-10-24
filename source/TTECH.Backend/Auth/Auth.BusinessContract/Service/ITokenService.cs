using Auth.BusinessContract.DTO;
using Auth.RepositoryContract.Entity;

namespace Auth.BusinessContract.Service
{
  public interface ITokenService
  {
    TokensResponse? GenerateTokenPair(CoreUser user);
  }
}
