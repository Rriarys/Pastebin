using Pastebin.Interfaces;
using Pastebin.Utils;
using HashidsNet;

namespace Pastebin.Services
{
    public class HashService : IHashService
    {
        private readonly Hashids _hashids;

        public HashService(string salt, int minHashLength)
        {
            _hashids = new Hashids(salt, minHashLength);
        }

        public string GenerateHash(int id)
        {
            return _hashids.Encode(id);
        }

        public int DecodeHash(string hash)
        {
            var decoded = _hashids.Decode(hash);

            return decoded.Length > 0 ? decoded[0] :
                throw new InvalidOperationException("Invalid hash");
        }
    }
}
