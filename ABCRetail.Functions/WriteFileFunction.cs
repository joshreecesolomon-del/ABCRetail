using Azure.Storage.Files.Shares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text;

namespace ABCRetail.Functions
{
    public class WriteFileFunction
    {
        [Function("WriteFile")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "post",
                Route = "write-file")] HttpRequestData req)
        {
            string? connectionString =
                Environment.GetEnvironmentVariable("ABCRetailStorage");

            if (string.IsNullOrEmpty(connectionString))
            {
                var errorResponse =
                    req.CreateResponse(HttpStatusCode.InternalServerError);

                await errorResponse.WriteStringAsync(
                    "Azure Storage connection string is missing.");

                return errorResponse;
            }

            // Read the text sent from Postman
            string content =
                await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                var badRequest =
                    req.CreateResponse(HttpStatusCode.BadRequest);

                await badRequest.WriteStringAsync(
                    "Please provide file content.");

                return badRequest;
            }

            // Connect to the existing Azure File Share
            ShareClient shareClient =
                new ShareClient(
                    connectionString,
                    "application-logs");

            await shareClient.CreateIfNotExistsAsync();

            ShareDirectoryClient directoryClient =
                shareClient.GetRootDirectoryClient();

            // Give every test file a unique name
            string fileName =
                $"function-log-{DateTime.UtcNow:yyyyMMdd-HHmmss}.txt";

            ShareFileClient fileClient =
                directoryClient.GetFileClient(fileName);

            byte[] data = Encoding.UTF8.GetBytes(content);

            // Create the Azure file with the required size
            await fileClient.CreateAsync(data.Length);

            using MemoryStream stream = new MemoryStream(data);

            // Upload the content into the file
            await fileClient.UploadRangeAsync(
                new Azure.HttpRange(0, data.Length),
                stream);

            var response =
                req.CreateResponse(HttpStatusCode.OK);

            await response.WriteStringAsync(
                $"File successfully written to Azure Files: {fileName}");

            return response;
        }
    }
}