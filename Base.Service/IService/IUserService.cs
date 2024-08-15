using Base.Repository.Identity;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;

namespace Base.Service.IService;

public interface IUserService
{
    Task<LoginUserManagement> LoginWithGoogle(string accessToken);
    Task<LoginUserManagement> LoginUser(LoginUserVM resource);
    Task<ServiceResponseVM<User>> CreateNewUser(UserVM newEntity);
    Task<User?> GetUserById(Guid id);
    Task<UserManagerResponse> ResetPassword(ResetPasswordVM resource);
    Task<UserManagerResponse> ForgetPassword(string email);
    Task<UserManagerResponse> ForgetAndResetPasswordAsync(ForgetPasswordVM model);
    Task<ServiceResponseVM<User>> UpdateUser(Guid userId, UpdateUserVM updatedUser);
}
