
using Dapr.AppCallback.Autogen.Grpc.v1;
using Dapr.Client.Autogen.Grpc.v1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace GreetingGRPC
{
    public class AppCallbackService : Dapr.AppCallback.Autogen.Grpc.v1.AppCallback.AppCallbackBase
    {
        public override Task<InvokeResponse> OnInvoke(InvokeRequest request, ServerCallContext context)
        {
            switch (request.Method)
            {
                case "SayHello":
                    {
                        var dataString = request.Data.Value.ToStringUtf8();

                        var dataDefinition = new { Name = "" };
                        var data = JsonConvert.DeserializeAnonymousType(dataString, dataDefinition);

                        var result = new { result = $"Hello, {data.Name}!" };
                        return Task.FromResult(new InvokeResponse()
                        {
                            ContentType = "application/json",
                            Data = new Any()
                            {
                                Value = ByteString.CopyFrom(JsonConvert.SerializeObject(result),
                                Encoding.UTF8)
                            }
                        });
                    }
                default: return base.OnInvoke(request, context);
            }
        }

        public override Task<ListTopicSubscriptionsResponse> ListTopicSubscriptions(Empty request, ServerCallContext context) =>
            Task.FromResult(new ListTopicSubscriptionsResponse());
    }
}