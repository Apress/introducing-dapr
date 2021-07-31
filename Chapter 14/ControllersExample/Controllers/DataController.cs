using Common;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AspNetCoreExample.Controllers
{
    [ApiController]
    public class DataController : ControllerBase
    {
        public DataController(ILogger<DataController> logger)
        {
            this.logger = logger;
        }

        private readonly ILogger<DataController> logger;

        [HttpGet("/data/{id}")]
        public ActionResult<DummyData> GetDataFromStateStore([FromState(Constants.StateStoreName, "id")] StateEntry<DummyData> data)
        {
            if (data.Value is null)
            {
                return this.NotFound();
            }

            return data.Value;
        }

        [Topic(Constants.PubSubName, Constants.TopicName)]
        [HttpPost(Constants.TopicName)]
        public async Task<ActionResult> ReceiveMessageFromTopic(DummyData newData, [FromServices] DaprClient daprClient)
        {
            await daprClient.SaveStateAsync<DummyData>(Constants.StateStoreName, newData.Id, newData);

            return Ok();
        }
    }
}
