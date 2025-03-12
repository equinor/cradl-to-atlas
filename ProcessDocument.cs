using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Identity;

namespace cradl_to_atlas
{
    public class ProcessDocument
    {
        private readonly ILogger<ProcessDocument> _logger;

        public ProcessDocument(ILogger<ProcessDocument> logger)
        {
            _logger = logger;
        }

        [Function("ProcessDocument")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("Received request to store JSON data in Blob Storage.");

        try
        {
            // Read JSON payload from HTTP request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Validate JSON
            if (string.IsNullOrEmpty(requestBody))
            {
                _logger.LogWarning("Empty request body received.");
                return new BadRequestObjectResult("Invalid request: JSON payload is required.");
            }

            // Generate a unique filename with timestamp
            string fileName = $"data-{DateTime.UtcNow:yyyyMMddHHmmss}.json";
            string storageAccountUrl = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_URL");
            if (string.IsNullOrEmpty(storageAccountUrl))
            {
                _logger.LogError("Storage account URL is not configured.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            string containerName = "cradlai-json"; // Change this to your blob container name

            // Authenticate using Managed Identity (MSAL)
            var blobServiceClient = new BlobServiceClient(new Uri(storageAccountUrl), new DefaultAzureCredential());
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            // Get a reference to the blob
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            // Upload JSON data to Blob Storage
            using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody)))
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = "application/json" });
            }

            _logger.LogInformation($"JSON data successfully stored in Blob Storage as {fileName}");

            return new OkObjectResult(new { message = "Data successfully stored", fileName = fileName });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error storing JSON to Blob: {ex.Message}");
            return new StatusCodeResult(500);
        }
        }
    }
}
