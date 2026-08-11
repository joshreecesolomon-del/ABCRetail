using Azure.Storage.Files.Shares;
using System.Text;

namespace ABCRetail.Services
{
    public class FileStorageService
    {
        private readonly ShareClient _shareClient;

        public FileStorageService(IConfiguration configuration)
        {
            string connectionString =
                configuration.GetConnectionString("AzureStorage");

            _shareClient = new ShareClient(
                connectionString,
                "application-logs");
        }

        public async Task LogAsync(string message)
        {
            await _shareClient.CreateIfNotExistsAsync();

            ShareDirectoryClient directory =
                _shareClient.GetRootDirectoryClient();

            ShareFileClient file =
                directory.GetFileClient("application.log");

            string logEntry =
                $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC - {message}"
                + Environment.NewLine;

            byte[] newLogBytes = Encoding.UTF8.GetBytes(logEntry);

            if (await file.ExistsAsync())
            {
                var properties = await file.GetPropertiesAsync();

                long existingLength = properties.Value.ContentLength;

                await file.CreateAsync(
                    existingLength + newLogBytes.Length);

                using MemoryStream stream =
                    new MemoryStream(newLogBytes);

                await file.UploadRangeAsync(
                    new Azure.HttpRange(
                        existingLength,
                        newLogBytes.Length),
                    stream);
            }
            else
            {
                await file.CreateAsync(newLogBytes.Length);

                using MemoryStream stream =
                    new MemoryStream(newLogBytes);

                await file.UploadRangeAsync(
                    new Azure.HttpRange(
                        0,
                        newLogBytes.Length),
                    stream);
            }
        }
    }
}