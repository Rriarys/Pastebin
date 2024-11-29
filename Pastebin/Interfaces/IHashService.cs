namespace Pastebin.Interfaces
{
    public interface IHashService
    {
        string GenerateHash(string id);

        int DecodeHash(string hash);
    }
}
