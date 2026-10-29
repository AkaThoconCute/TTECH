using Auth.BusinessContract.DTOs;
using Auth.BusinessContract.Enums;
using Auth.RepositoryContract.Entity;

namespace Auth.BusinessContract.Services
{
  public interface IUserService
  {
    public Task<CoreUser?> CreateUser(
      SignUpRequest signupRequest, 
      RoleEnum role = RoleEnum.User);
  }
}
