using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;

namespace ABCRetail.Functions
{
    public class StoreCustomerFunction
    {
        [Function("StoreCustomer")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "post",
                Route = "customers")] HttpRequestData req)
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

            string requestBody =
                await new StreamReader(req.Body).ReadToEndAsync();

            CustomerRequest? customer =
                JsonSerializer.Deserialize<CustomerRequest>(
                    requestBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (customer == null)
            {
                var badRequest =
                    req.CreateResponse(HttpStatusCode.BadRequest);

                await badRequest.WriteStringAsync(
                    "Customer information is required.");

                return badRequest;
            }

            TableClient tableClient =
                new TableClient(
                    connectionString,
                    "Customers");

            await tableClient.CreateIfNotExistsAsync();

            var entity = new TableEntity(
                "Customer",
                Guid.NewGuid().ToString())
            {
                ["FirstName"] = customer.FirstName ?? "",
                ["LastName"] = customer.LastName ?? "",
                ["Email"] = customer.Email ?? "",
                ["PhoneNumber"] = customer.PhoneNumber ?? "",
                ["Address"] = customer.Address ?? "",
                ["RegistrationDate"] = DateTimeOffset.UtcNow
            };

            await tableClient.AddEntityAsync(entity);

            var response =
                req.CreateResponse(HttpStatusCode.OK);

            await response.WriteStringAsync(
                "Customer stored successfully in Azure Table Storage.");

            return response;
        }
    }

    public class CustomerRequest
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }
    }
}