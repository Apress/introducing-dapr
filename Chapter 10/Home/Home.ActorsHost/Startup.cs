using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.ActorsHost.Actors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Home.ActorsHost
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Home.ActorsHost", Version = "v1" });
            });
            services.AddActors(actorRuntime =>
            {
                actorRuntime.ActorIdleTimeout = TimeSpan.FromHours(1);
                actorRuntime.ActorScanInterval = TimeSpan.FromSeconds(30);
                actorRuntime.DrainOngoingCallTimeout = TimeSpan.FromSeconds(40);
                actorRuntime.DrainRebalancedActors = true;

                actorRuntime.Actors.RegisterActor<RoomActor>();
                actorRuntime.Actors.RegisterActor<AirConActor>();
                actorRuntime.Actors.RegisterActor<TemperatureSensorActor>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Home.ActorsHost v1"));
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapActorsHandlers();
            });
        }
    }
}
