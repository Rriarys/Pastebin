using Azure.Storage.Blobs;
using Pastebin.Interfaces;

namespace Pastebin.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<string> UploadTextAsync(string containerName, string fileName, string content)
        {
            // Нормализуем имя контейнера (убираем недопустимые символы)
            containerName = NormalizeBlobName(containerName);
            fileName = NormalizeBlobName(fileName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            try
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                throw new InvalidOperationException("Error uploading blob", ex);
            }

            return blobClient.Uri.ToString();
        }

        public async Task<string> GetBlobUrlAsync(string containerName, string fileName)
        {
            containerName = NormalizeBlobName(containerName);
            fileName = NormalizeBlobName(fileName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            return blobClient.Uri.ToString();
        }

        // Реализация метода удаления файла
        public async Task DeleteBlobAsync(string containerName, string fileName)
        {
            containerName = NormalizeBlobName(containerName);
            fileName = NormalizeBlobName(fileName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            // Удаляем файл, если он существует
            await blobClient.DeleteIfExistsAsync();
        }

        private string NormalizeBlobName(string name)
        {
            // Нормализуем имя: делаем его строчным и заменяем пробелы и другие недопустимые символы
            return name.ToLower().Replace(" ", "-").Replace("_", "-");
        }
    }
}
