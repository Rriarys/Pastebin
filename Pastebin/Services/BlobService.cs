﻿using Azure.Storage.Blobs;
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
            containerName = NormalizeBlobName(containerName);
            fileName = NormalizeBlobName(fileName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(); // Создает контейнер, если он отсутствует.

            var blobClient = containerClient.GetBlobClient(fileName);

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            try
            {
                // Загружаем текст в blob.
                await blobClient.UploadAsync(stream, overwrite: true);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error uploading blob", ex);
            }

            return blobClient.Uri.ToString(); // Возвращает прямой URI до файла.
        }

        public async Task<string> GetBlobUrlAsync(string containerName, string fileName)
        {
            containerName = NormalizeBlobName(containerName);
            fileName = NormalizeBlobName(fileName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            // Прямой URL для blob объекта.
            return blobClient.Uri.ToString();
        }

        public async Task<byte[]> GetBlobContentAsync(string containerName, string postHash)
        {
            var blobClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blob = blobClient.GetBlobClient(postHash);

            if (!await blob.ExistsAsync())
            {
                return null; // Файл не найден
            }

            var download = await blob.DownloadAsync();
            using (var memoryStream = new MemoryStream())
            {
                await download.Value.Content.CopyToAsync(memoryStream);
                return memoryStream.ToArray(); // Возвращаем содержимое файла
            }
        }

        public async Task DeleteBlobAsync(string containerName, string fileName)
        {
            containerName = NormalizeBlobName(containerName);
            fileName = NormalizeBlobName(fileName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            // Удаляет blob, если он существует.
            await blobClient.DeleteIfExistsAsync();
        }

        private string NormalizeBlobName(string name)
        {
            // Преобразует имя в нижний регистр и заменяет пробелы и подчеркивания на тире.
            return name.ToLower().Replace(" ", "-").Replace("_", "-");
        }
        public async Task<bool> IsBlobStorageAvailableAsync(string userName)
        {
            try
            {
                // Нормализуем имя пользователя и получаем контейнер для него
                string containerName = NormalizeBlobName(userName.ToLower());

                // Проверяем контейнер, принадлежащий пользователю
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                // Пытаемся получить хотя бы один объект в контейнере
                await foreach (var blobItem in containerClient.GetBlobsAsync())
                {
                    // Если можем получить хотя бы один объект, значит контейнер доступен
                    return true;
                }
            }
            catch (Exception)
            {
                // В случае ошибки доступ к контейнеру невозможен
                return false;
            }
            return false; // Контейнер пуст или ошибка доступа
        }
    }
}
