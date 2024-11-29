namespace Pastebin.Configuration
{
    public class HashOptions
    {
        public string Salt { get; set; } = "defaul_salt";

        public int MinHashLength { get; set; } = 8;
    }
}
