using API;
using DAL;
using Domain;
using Domain.entity;
using Domain.VM;
using Microsoft.EntityFrameworkCore;
using Services.Helpers;
using Services.Interfaces;

namespace Services.Implemt
{
    public class TokenService : ITokenService
    {
        private readonly AppDbContext dbContext;

        public TokenService(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Tuple<string, string>> GenerateTokensAsync(int userId)
        {
            var accessToken = await TokenHelper.GenerateAccessToken(userId);
            var refreshToken = await TokenHelper.GenerateRefreshToken();

            var userRecord = await dbContext.Users.Include(o => o.RefreshTokens).FirstOrDefaultAsync(e => e.Id == userId);

            if (userRecord == null)
            {
                return null;
            }

            var salt = PasswordHelper.GetSecureSalt();

            var refreshTokenHashed = PasswordHelper.HashUsingPbkdf2(refreshToken, salt);

            if (userRecord.RefreshTokens != null && userRecord.RefreshTokens.Any())
            {
                await RemoveRefreshTokenAsync(userRecord);
            }
            userRecord.RefreshTokens?.Add(new RefreshToken
            {
                ExpiryDate = DateTime.Now.AddDays(30),
                Ts = DateTime.Now,
                UserId = userId,
                TokenHash = refreshTokenHashed,
                TokenSalt = Convert.ToBase64String(salt)
            });

            await dbContext.SaveChangesAsync();

            var token = new Tuple<string, string>(accessToken, refreshToken);

            return token;        
        }

        public async Task<string> RefreshAccessToken(string refreshToken)
        {
            var refreshTokenFromDB = await dbContext.RefreshTokens
               .FirstOrDefaultAsync(o => o.TokenHash == refreshToken);

            if (refreshTokenFromDB == null)
            {
                return null;
            }

            var accessToken = await TokenHelper.GenerateAccessToken(refreshTokenFromDB.UserId);

            return accessToken;
        }

        public async Task<bool> RemoveRefreshTokenAsync(User user)
        {
            var userRecord = dbContext.Users.FirstOrDefault(e => e.Id == user.Id);

            if (userRecord == null)
            {
                return false;
            }

            if (userRecord.RefreshTokens != null && userRecord.RefreshTokens.Any())
            {
                var currentRefreshToken = userRecord.RefreshTokens.First();

                dbContext.RefreshTokens.Remove(currentRefreshToken);

                await dbContext.SaveChangesAsync();
            }

            return false;
        }

        public async Task<BaseResponse<int>> ValidateRefreshTokenAsync(string refreshToken)
        {
            var RefreshTokenFromDB = await dbContext.RefreshTokens
                .FirstOrDefaultAsync(o => o.TokenHash == refreshToken);

            var response = new BaseResponse<int>();

            if (RefreshTokenFromDB == null)
            {
                response.StatusCode = Domain.enums.StatusCode.NotFound;
                response.Description = "Invalid session or user is already logged out";
                return response;
            }

            var refreshTokenToValidateHash = PasswordHelper
                .HashUsingPbkdf2(refreshToken, Convert.FromBase64String(RefreshTokenFromDB.TokenSalt));

            if (RefreshTokenFromDB.TokenHash != refreshTokenToValidateHash)
            {
                response.StatusCode = Domain.enums.StatusCode.NotFound;
                response.Description = "Invalid refresh token";
                return response;
            }
          
            if (RefreshTokenFromDB.ExpiryDate < DateTime.Now)
            {
                response.StatusCode =  Domain.enums.StatusCode.NotFound;
                response.Description = "Refresh token has expired";
                return response;
            }

            response.StatusCode = Domain.enums.StatusCode.Ok;
            response.Data = RefreshTokenFromDB.UserId;

            return response;
        }
    }
}