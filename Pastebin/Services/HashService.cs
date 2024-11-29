using Pastebin.Interfaces;
using HashidsNet;
using System.Text;
using System.Security.Cryptography;

namespace Pastebin.Services
{
    public class HashService : IHashService
    {
        private readonly Hashids _hashids;

        public HashService(string salt, int minHashLength)
        {
            _hashids = new Hashids(salt, minHashLength);
        }

        public string GenerateHash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        public int DecodeHash(string hash)
        {
            var decoded = _hashids.Decode(hash);

            return decoded.Length > 0 ? decoded[0] :
                throw new InvalidOperationException("Invalid hash");
        }
    }
}
