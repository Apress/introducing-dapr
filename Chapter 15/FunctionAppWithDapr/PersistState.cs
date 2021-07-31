using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Dapr.AzureFunctions.Extension;
using CloudNative.CloudEvents;

namespace FunctionAppWithDapr
{
    public static class PersistState
    {
        [FunctionName("PersistState")]
        public static void Run(
            [DaprTopicTrigger("%PubSubName%", Topic = "%TargetTopicName%")] CloudEvent inputEvent,
            [DaprState("%StateStoreName%", Key = "%StateItemKey%")] out object state,
            ILogger log)
        {
            log.LogInformation("C# function received an event by a topic trigger from" +
                " Dapr Runtime with payload: " + inputEvent.Data);
            log.LogInformation($"Persisting the payload into a state store");

            state = inputEvent.Data;
        }
    }
}
