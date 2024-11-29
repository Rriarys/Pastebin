namespace Pastebin.Interfaces
{
    public interface IHashService
    {
        string GenerateHash(int id);

        int DecodeHash(string hash);
    }
}
