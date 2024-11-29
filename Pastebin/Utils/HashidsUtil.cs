using HashidsNet;
using Pastebin.Configuration;

namespace Pastebin.Utils
{
    public class HashidsUtil
    {
        public static Hashids CreateHashids(HashOptions options)
        {
            return new Hashids(options.Salt, options.MinHashLength);
        }
    }
}
