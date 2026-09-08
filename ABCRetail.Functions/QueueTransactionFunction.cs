using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace ABCRetail.Functions
{
    public class QueueTransactionFunction
    {
        [Function("QueueTransaction")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "get",
                "post",
                Route = "transactions")] HttpRequestData req)
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

            QueueClient queueClient =
                new QueueClient(
                    connectionString,
                    "order-processing");

            await queueClient.CreateIfNotExistsAsync();

            // POST = Write a message to the queue
            if (req.Method.Equals(
                "POST",
                StringComparison.OrdinalIgnoreCase))
            {
                string message =
                    await new StreamReader(req.Body).ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(message))
                {
                    var badRequest =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await badRequest.WriteStringAsync(
                        "Please provide transaction information.");

                    return badRequest;
                }

                await queueClient.SendMessageAsync(message);

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteStringAsync(
                    "Transaction successfully added to the order-processing queue.");

                return response;
            }

            // GET = Read a message from the queue
            var receivedMessage =
                await queueClient.ReceiveMessageAsync();

            if (receivedMessage.Value == null)
            {
                var emptyResponse =
                    req.CreateResponse(HttpStatusCode.OK);

                await emptyResponse.WriteStringAsync(
                    "No transaction messages are currently in the queue.");

                return emptyResponse;
            }

            string messageText =
                receivedMessage.Value.MessageText;

            var readResponse =
                req.CreateResponse(HttpStatusCode.OK);

            await readResponse.WriteStringAsync(
                $"Transaction read from queue: {messageText}");

            return readResponse;
        }
    }
}