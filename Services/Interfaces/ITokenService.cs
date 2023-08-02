using Domain;
using Domain.entity;
using Domain.VM;

namespace Services.Interfaces
{
    public interface ITokenService
    {
        Task<Tuple<string, string>> GenerateTokensAsync(int userId);
        Task<BaseResponse<int>> ValidateRefreshTokenAsync(RefreshTokenVM refreshToken);
        Task<bool> RemoveRefreshTokenAsync(User user);
    }
}