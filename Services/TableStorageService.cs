using Azure.Data.Tables;

namespace ABCRetail.Services
{
    public class TableStorageService
    {
        private readonly TableServiceClient _tableService;

        public TableStorageService(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("AzureStorage");

            _tableService = new TableServiceClient(connectionString);
        }

        public TableClient GetTable(string tableName)
        {
            return _tableService.GetTableClient(tableName);
        }
    }
}