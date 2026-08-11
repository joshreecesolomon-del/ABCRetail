using Azure.Storage.Blobs;

namespace ABCRetail.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _container;

        public BlobStorageService(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("AzureStorage");

            _container = new BlobContainerClient(
                connectionString,
                "product-images");

            _container.CreateIfNotExists();
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

            var blobClient = _container.GetBlobClient(fileName);

            var blob = _container.GetBlobClient(file.FileName);

            using var stream = file.OpenReadStream();

            await blob.UploadAsync(stream, overwrite: true);

            return blob.Uri.ToString();
        }
    }
}