using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace ABCRetail.Functions
{
    public class UploadProductImageFunction
    {
        [Function("UploadProductImage")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "post",
                Route = "upload-image")] HttpRequestData req)
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

            string? fileName = req.Headers
                .FirstOrDefault(h => h.Key == "x-file-name")
                .Value?
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(fileName))
            {
                var badRequest =
                    req.CreateResponse(HttpStatusCode.BadRequest);

                await badRequest.WriteStringAsync(
                    "Please provide the file name in the x-file-name header.");

                return badRequest;
            }

            BlobContainerClient containerClient =
                new BlobContainerClient(
                    connectionString,
                    "product-images");

            await containerClient.CreateIfNotExistsAsync();

            string uniqueFileName =
                $"{Guid.NewGuid()}-{fileName}";

            BlobClient blobClient =
                containerClient.GetBlobClient(uniqueFileName);

            await blobClient.UploadAsync(
                req.Body,
                overwrite: true);

            var response =
                req.CreateResponse(HttpStatusCode.OK);

            await response.WriteStringAsync(
                $"Image uploaded successfully: {uniqueFileName}");

            return response;
        }
    }
}