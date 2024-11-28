using System.Security.Cryptography;
using System.Text;

namespace Pastebin.Helpers
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
    }

    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            // Простой пример хеширования пароля
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }

}
