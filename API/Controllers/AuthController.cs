using System.Linq;
using Domain.VM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly ITokenService tokenService;

        public AuthController(IUserService userService, ITokenService tokenService)
        {
            this.userService = userService;
            this.tokenService = tokenService;
        }

        [Authorize]
        [HttpGet]
        [Route("test")]
        public async Task<IActionResult> Test()
        {
            return Ok("Авторизованы");
        }


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(UserLoginVM userVM)
        {
            if (userVM == null || string.IsNullOrEmpty(userVM.Email) || 
                string.IsNullOrEmpty(userVM.Password))
            {
                return BadRequest("Missing login details");
            }

            var loginResponse = await userService.LoginAsync(userVM);

            if (loginResponse.StatusCode != Domain.enums.StatusCode.Ok)
            {
                return Unauthorized(loginResponse);
            }
            HttpContext.Response.Cookies.Append("refreshToken", loginResponse.Data.Item1);

            return Ok(loginResponse);
        }

        [HttpPost]
        [Route("refresh_token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenVM refreshTokenVM)
        {
            if (refreshTokenVM == null || string.IsNullOrEmpty
            (refreshTokenVM.RefreshToken) || refreshTokenVM.UserId == 0)
            {
                return BadRequest("Missing refresh token details");
            }

            var validateRefreshTokenResponse = 
                await tokenService.ValidateRefreshTokenAsync(refreshTokenVM.RefreshToken);

            if (validateRefreshTokenResponse.StatusCode != Domain.enums.StatusCode.Ok)
            {
                return UnprocessableEntity(validateRefreshTokenResponse);
            }

            var tokenResponse = await tokenService
                .GenerateTokensAsync(validateRefreshTokenResponse.Data);

            return Ok(new { AccessToken = tokenResponse.Item1, 
                            Refreshtoken = tokenResponse.Item2 });
        }

        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> Signup(UserRegistrVM userVM)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(x => x.Errors.Select(c => c.ErrorMessage)).ToList();

                if (errors.Any())
                {
                    return BadRequest($"{string.Join(",", errors)}");
                }
            }
         
            var signupResponse = await userService.SignupAsync(userVM);

            if (signupResponse.StatusCode != Domain.enums.StatusCode.Ok)
            {
                return UnprocessableEntity(signupResponse);
            }

            return Ok(signupResponse.Data);
        }

        [Authorize]
        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout(int userId)
        {
            var logout = await userService.LogoutAsync(userId);

            if (logout.StatusCode != Domain.enums.StatusCode.Ok)
            {
                return UnprocessableEntity(logout);
            }

            return Ok();
        }
    }
}