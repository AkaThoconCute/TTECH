using Auth.BusinessContract.DTO;
using Auth.BusinessContract.Enum;
using Auth.RepositoryContract.Entity;

namespace Auth.BusinessContract.Service
{
  public interface IUserService
  {
    public Task<CoreUser?> CreateUser(
      SignUpRequest signupRequest, 
      RoleEnum role = RoleEnum.User);
  }
}
