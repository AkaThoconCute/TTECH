using Auth.BusinessContract.DTO;
using Auth.BusinessContract.Enum;
using Auth.BusinessContract.Service;
using Auth.RepositoryContract.Entity;
using Microsoft.AspNetCore.Identity;

namespace Auth.BusinessImplement.Service
{
  public class UserServiceImpl(
    UserManager<CoreUser> coreUserManager) : IUserService
  {
    public async Task<CoreUser?> CreateUser(
      SignUpRequest signupRequest,
      RoleEnum role = RoleEnum.User)
    {
      // 1. Create a new user
      CoreUser user = new()
      {
        UserName = signupRequest.Username,
        FullName = signupRequest.FullName,
        Email = signupRequest.Email,
        PhoneNumber = signupRequest.Phone,
        Address = signupRequest.Address,
      };

      string password = signupRequest.Password;

      IdentityResult createRes = await coreUserManager.CreateAsync(user, password);

      if (createRes.Succeeded is false)
      {
        string message = string.Join(" ", createRes.Errors.Select(e => e.Description));
        return null;
      }

      // 2. Update the user to add role
      //IdentityResult updateRes = await userManager.AddToRoleAsync(user, role.ToString());

      if (createRes.Succeeded is false)
      {
        string message = string.Join(", ", createRes.Errors.Select(e => e.Description));
        return null;
      }

      // 3. Response
      CoreUser? createdUser = await coreUserManager.FindByNameAsync(user.UserName);
      if (createdUser is null) return null;

      return createdUser;
    }
  }
}
