
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDayAPI.Services
{
    public class BlobStorageService
    {
        private readonly IConfiguration _configuration;

        public BlobStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> UploadImageAsync(
            IFormFile image,
            string containerSetting)
        {
            if (image == null || image.Length == 0)
            {
                throw new ArgumentException("Please select an image.");
            }

            if (image.Length > 5 * 1024 * 1024)
            {
                throw new ArgumentException(
                    "The image must not exceed 5 MB.");
            }

            var allowedTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            if (!allowedTypes.Contains(image.ContentType))
            {
                throw new ArgumentException(
                    "Only JPG, PNG and WEBP images are allowed.");
            }

            var connectionString =
                _configuration["AzureBlobStorage:ConnectionString"];

            var containerName =
                _configuration[$"AzureBlobStorage:{containerSetting}"];

            if (string.IsNullOrWhiteSpace(connectionString) ||
                string.IsNullOrWhiteSpace(containerName))
            {
                throw new InvalidOperationException(
                    "Azure Blob Storage configuration is missing.");
            }

            var containerClient = new BlobContainerClient(
                connectionString, containerName);

            await containerClient.CreateIfNotExistsAsync(
                PublicAccessType.None);

            var extension = Path.GetExtension(image.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";

            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = image.OpenReadStream())
            {
                await blobClient.UploadAsync(
                    stream,
                    new BlobHttpHeaders
                    {
                        ContentType = image.ContentType
                    });
            }

            return fileName;
        }

        public async Task<Stream?> DownloadImageAsync(
            string fileName,
            string containerSetting)
        {
            var connectionString =
                _configuration["AzureBlobStorage:ConnectionString"];

            var containerName =
                _configuration[$"AzureBlobStorage:{containerSetting}"];

            if (string.IsNullOrWhiteSpace(connectionString) ||
                string.IsNullOrWhiteSpace(containerName))
            {
                throw new InvalidOperationException(
                    "Azure Blob Storage configuration is missing.");
            }

            var containerClient = new BlobContainerClient(
                connectionString, containerName);

            var blobClient = containerClient.GetBlobClient(fileName);

            if (!await blobClient.ExistsAsync())
            {
                return null;
            }

            var download = await blobClient.DownloadStreamingAsync();

            return download.Value.Content;
        }

        public async Task DeleteImageAsync(
            string? fileName,
            string containerSetting)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            var connectionString =
                _configuration["AzureBlobStorage:ConnectionString"];

            var containerName =
                _configuration[$"AzureBlobStorage:{containerSetting}"];

            if (string.IsNullOrWhiteSpace(connectionString) ||
                string.IsNullOrWhiteSpace(containerName))
            {
                throw new InvalidOperationException(
                    "Azure Blob Storage configuration is missing.");
            }

            var containerClient = new BlobContainerClient(
                connectionString, containerName);

            var blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync();
        }
    }
}
