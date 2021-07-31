using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Dapr.AzureFunctions.Extension;

namespace FunctionAppWithDapr
{
    public static class GetState
    {
        [FunctionName("GetState")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "state/{key}")] HttpRequest req,
            [DaprState("%StateStoreName%", Key = "{key}")] string state,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            return new OkObjectResult(state);
        }
    }
}
