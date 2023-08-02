using Domain;
using Domain.VM;

namespace Services.Interfaces
{
    public interface IUserService
    {
        Task<BaseResponse<Tuple<string, string>>> LoginAsync(UserLoginVM userVM);
        Task<BaseResponse<bool>> SignupAsync(UserRegistrVM userVM);
        Task<BaseResponse<bool>> LogoutAsync(int userId);
    }
}