using Dapr.Client;
using Dapr.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaprSecretStoreExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var client = new DaprClientBuilder().Build();

            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((services) =>
                {
                    services.AddSingleton<DaprClient>(client);
                })
                .ConfigureAppConfiguration((configBuilder) =>
                {
                    // List the secrets that you want to retieve from the secret store.
                    var secretDescriptors = new DaprSecretDescriptor[]
                    {
                        new DaprSecretDescriptor("very-secret")
                    };

                    // Add the secret store Configuration Provider to the configuration builder.
                    configBuilder.AddDaprSecretStore("mysecrets", secretDescriptors, client);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
