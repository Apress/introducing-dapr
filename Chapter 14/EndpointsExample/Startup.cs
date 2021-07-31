using Common;
using Dapr.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;

namespace EndpointsExample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDaprClient();

            services.AddSingleton(new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, JsonSerializerOptions serializerOptions)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCloudEvents();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapSubscribeHandler();

                endpoints.MapGet("/data/{id}", GetDataFromStateStore);
                endpoints.MapPost(Constants.TopicName, ReceiveMessageFromTopic).WithTopic(Constants.PubSubName, Constants.TopicName);
            });

            async Task GetDataFromStateStore(HttpContext context)
            {
                var client = context.RequestServices.GetRequiredService<DaprClient>();

                var id = (string)context.Request.RouteValues["id"];
                var data = await client.GetStateAsync<DummyData>(Constants.StateStoreName, id);
                if (data == null)
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                context.Response.ContentType = MediaTypeNames.Application.Json;
                await JsonSerializer.SerializeAsync(context.Response.Body, data, serializerOptions);
            }

            async Task ReceiveMessageFromTopic(HttpContext context)
            {
                var client = context.RequestServices.GetRequiredService<DaprClient>();

                var newData = await JsonSerializer.DeserializeAsync<DummyData>(context.Request.Body, serializerOptions);
                await client.SaveStateAsync<DummyData>(Constants.StateStoreName, newData.Id, newData);
                
                context.Response.StatusCode = 200;
            }
        }
    }
}
