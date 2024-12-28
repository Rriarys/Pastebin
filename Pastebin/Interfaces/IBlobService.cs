namespace Pastebin.Interfaces
{
    public interface IBlobService
    {
        Task<string> UploadTextAsync(string containerName, string fileName, string content);

        Task<string> GetBlobUrlAsync(string containerName, string fileName);

        Task<byte[]> GetBlobContentAsync(string containerName, string postHash);

        Task DeleteBlobAsync(string containerName, string fileName);

        Task<bool> IsBlobStorageAvailableAsync(string userName);
    }
}
