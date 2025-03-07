using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
