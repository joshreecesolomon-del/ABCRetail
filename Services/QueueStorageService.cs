using Azure.Storage.Queues;
using System.Text.Json;

namespace ABCRetail.Services
{
    public class QueueStorageService
    {
        private readonly QueueServiceClient _queueService;

        public QueueStorageService(IConfiguration configuration)
        {
            string connectionString =
                configuration.GetConnectionString("AzureStorage");

            _queueService = new QueueServiceClient(connectionString);
        }

        public async Task SendMessageAsync(
            string queueName,
            string message)
        {
            QueueClient queue = _queueService.GetQueueClient(queueName);

            await queue.CreateIfNotExistsAsync();

            await queue.SendMessageAsync(message);
        }
    }
}