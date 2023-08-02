using DAL;
using Domain;
using Domain.entity;
using Domain.VM;
using Microsoft.EntityFrameworkCore;
using Services.Helpers;
using Services.Interfaces;

namespace Services.Implemt
{
    public class UserService : IUserService
    {
        private readonly AppDbContext dbContext;
        private readonly ITokenService tokenService;

        public UserService(AppDbContext dbContext, ITokenService tokenService)
        {
            this.dbContext = dbContext;
            this.tokenService = tokenService;
        }

        public async Task<BaseResponse<Tuple<string, string>>> LoginAsync(UserLoginVM userVM)
        {
            var user = dbContext.Users
                .SingleOrDefault(user => user.Email == userVM.Email);

            if (user == null)
            {
                return new BaseResponse<Tuple<string, string>>
                {
                    StatusCode = Domain.enums.StatusCode.NotFound,
                    Description = "Email not found",
                };
            }

            var passwordHash = PasswordHelper
                .HashUsingPbkdf2(userVM.Password, Convert.FromBase64String(user.PasswordSalt));

            if (user.Password != passwordHash)
            {
                return new BaseResponse<Tuple<string, string>>
                {
                    StatusCode = Domain.enums.StatusCode.NotFound,
                    Description = "Invalid Password",
                };
            }

            var token = await Task.Run(() => 
                tokenService.GenerateTokensAsync(user.Id));

            return new BaseResponse<Tuple<string, string>>
            {
                StatusCode = Domain.enums.StatusCode.Ok,
                Data = token,
            };
        }

        public async Task<BaseResponse<bool>> LogoutAsync(int userId)
        {
            var refreshToken = await dbContext.RefreshTokens
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (refreshToken == null)
            {
                return new BaseResponse<bool> { StatusCode = Domain.enums.StatusCode.Ok };
            }

            dbContext.RefreshTokens.Remove(refreshToken);

            var saveResponse = await dbContext.SaveChangesAsync();

            if (saveResponse >= 0)
            {
                return new BaseResponse<bool> { StatusCode = Domain.enums.StatusCode.Ok };
            }

            return new BaseResponse<bool> 
            { 
                StatusCode = Domain.enums.StatusCode.NotFound, 
                Description = "Unable to logout user", 
            };
        }

        public async Task<BaseResponse<bool>> SignupAsync(UserRegistrVM userVM)
        {
            var existingUser = await dbContext.Users
                .SingleOrDefaultAsync(user => user.Email == userVM.Email);

            if (existingUser != null)
            {
                return new BaseResponse<bool>
                {
                    StatusCode = Domain.enums.StatusCode.NotFound,
                    Description = "User already exists with the same email",
                    Data = false,
                };
            }

            var salt = PasswordHelper.GetSecureSalt();
            var passwordHash = PasswordHelper.HashUsingPbkdf2(userVM.Password, salt);

            var user = new User
            {
                Email = userVM.Email,
                Password = passwordHash,
                PasswordSalt = Convert.ToBase64String(salt),
                FirstName = userVM.FirstName,
                LastName = userVM.LastName,
                Ts = userVM.Ts
               
            };

            await dbContext.Users.AddAsync(user);

            var saveResponse = await dbContext.SaveChangesAsync();

            if (saveResponse >= 0)
            {
                return new BaseResponse<bool> 
                { 
                    StatusCode = Domain.enums.StatusCode.Ok, 
                    Data = true
                };
            }

            return new BaseResponse<bool>
            {
                StatusCode = Domain.enums.StatusCode.NotFound,
                Description = "Unable to save the user",
                Data = false
            };
        }
    }
}