using Base.Repository.Identity;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;

namespace Base.Service.IService;

public interface IUserService
{
    Task<LoginUserManagement> LoginWithGoogle(string idToken);
    Task<LoginUserManagement> LoginUser(LoginUserVM resource);
    Task<ServiceResponseVM<User>> CreateNewUser(UserVM newEntity);
    Task<User?> GetUserById(Guid id);
}
