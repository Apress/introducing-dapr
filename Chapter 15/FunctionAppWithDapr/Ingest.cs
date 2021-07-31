using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Dapr.AzureFunctions.Extension;
using Newtonsoft.Json.Linq;

namespace FunctionAppWithDapr
{
    public static class Ingest
    {
        [FunctionName("Ingest")]
        public static void Run(
            [DaprServiceInvocationTrigger] JObject payload,
            [DaprPublish(PubSubName = "%PubSubName%", Topic = "%SourceTopicName%")] out object outputEvent,
            ILogger log)
        {
            log.LogInformation("C# function was invoked by Dapr with the following payload: " + payload);
            log.LogInformation("Sending the payload to a topic.");

            outputEvent = payload;
        }
    }
}
