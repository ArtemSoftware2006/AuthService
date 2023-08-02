using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace API
{
    public class TokenHelper
    {
        public static async Task<string> GenerateAccessToken(int userId)
        {
           var conf_builder = new ConfigurationBuilder();

            conf_builder.SetBasePath(Directory.GetCurrentDirectory());
            conf_builder.AddJsonFile("secutiry.json");
            var config = conf_builder.Build();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Convert.FromBase64String(config.GetSection("Secret").Value);

            //Тут добавляется полезная нагрузка
            var claimsIdentity = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            });

            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Issuer = config.GetSection("Issuer").Value,
                Audience = config.GetSection("Audience").Value,
                Expires = DateTime.Now.AddMinutes(15),
                SigningCredentials = signingCredentials,
            };
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);

            return await System.Threading.Tasks.Task.Run(() => 
                tokenHandler.WriteToken(securityToken));
        }
        public static async Task<string> GenerateRefreshToken()
        {
            var secureRandomBytes = new byte[32];

            using var randomNumberGenerator = RandomNumberGenerator.Create();
            await System.Threading.Tasks.Task.Run(() => 
                randomNumberGenerator.GetBytes(secureRandomBytes));

            var refreshToken = Convert.ToBase64String(secureRandomBytes);
            return refreshToken;
        }
    }
}