using Common;
using Dapr.AppCallback.Autogen.Grpc.v1;
using Dapr.Client;
using Dapr.Client.Autogen.Grpc.v1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GrpcServiceExample
{
    public class DataService : AppCallback.AppCallbackBase
    {
        private readonly ILogger<DataService> _logger;
        private readonly DaprClient _daprClient;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public DataService(DaprClient daprClient, ILogger<DataService> logger)
        {
            _daprClient = daprClient;
            _logger = logger;
        }


        public override async Task<InvokeResponse> OnInvoke(InvokeRequest request, ServerCallContext context)
        {
            var response = new InvokeResponse()
            {
                ContentType = "application/json"
            };
            switch (request.Method)
            {
                case "GetData":
                    var input = JsonSerializer.Deserialize<GetDataInput>(request.Data.Value.ToByteArray(), this._jsonOptions);
                    var output = await _daprClient.GetStateAsync<DummyData>(Constants.StateStoreName, input.Id);
                    response.Data = new Any
                    {
                        Value = ByteString.CopyFromUtf8(JsonSerializer.Serialize<DummyData>(output, this._jsonOptions)),
                    };
                    break;
                default:
                    break;
            }
            return response;
        }

        public override Task<ListTopicSubscriptionsResponse> ListTopicSubscriptions(Empty request, ServerCallContext context)
        {
            var result = new ListTopicSubscriptionsResponse();
            result.Subscriptions.Add(new TopicSubscription
            {
                PubsubName = Constants.PubSubName,
                Topic = Constants.TopicName
            });
            return Task.FromResult(result);
        }

        public override Task<ListInputBindingsResponse> ListInputBindings(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new ListInputBindingsResponse());
        }

        public override async Task<TopicEventResponse> OnTopicEvent(TopicEventRequest request, ServerCallContext context)
        {
            if (request.PubsubName == Constants.PubSubName)
            {
                var transaction = JsonSerializer.Deserialize<DummyData>(request.Data.ToStringUtf8(), this._jsonOptions);
                if (request.Topic == Constants.TopicName)
                {
                    await _daprClient.SaveStateAsync<DummyData>(Constants.StateStoreName, transaction.Id, transaction);
                }
            }

            return await Task.FromResult(new TopicEventResponse()
            {
                Status = TopicEventResponse.Types.TopicEventResponseStatus.Success
            });
        }
    }
}
