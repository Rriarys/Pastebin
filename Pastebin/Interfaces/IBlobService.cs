namespace Pastebin.Interfaces
{
    public interface IBlobService
    {
        Task<string> UploadTextAsync(string containerName, string fileName, string content);

        Task<string> GetBlobUrlAsync(string containerName, string fileName);
    }
}
