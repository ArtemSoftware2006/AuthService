using System.Security.Cryptography;
using System.Text;

namespace Services.Helpers
{
    public class HashPasswordHelper
    {
        public static byte[] GetSecureSalt()
        {
        
            return RandomNumberGenerator.GetBytes(32);
        }
         public static string HashPassword(string password)
        {
            var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
    }
}