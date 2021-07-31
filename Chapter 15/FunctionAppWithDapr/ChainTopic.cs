using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Dapr.AzureFunctions.Extension;
using CloudNative.CloudEvents;

namespace FunctionAppWithDapr
{
    public static class ChainTopic
    {
        [FunctionName("ChainTopic")]
        public static void Run(
            [DaprTopicTrigger("%PubSubName%", Topic = "%SourceTopicName%")] CloudEvent inputEvent,
            [DaprPublish(PubSubName = "%PubSubName%", Topic = "%TargetTopicName%")] out object outputEvent,
            ILogger log)
        {
            log.LogInformation("C# function received an event by a topic trigger from " +
                "Dapr with payload: " + inputEvent.Data);
            log.LogInformation($"Sending the event to another topic.");
            outputEvent = inputEvent.Data;
        }
    }
}
