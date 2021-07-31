using System;
using System.Text.Json;
using System.Threading.Tasks;
using Dapr.Client;

namespace TemperatureSensor
{
    class Program
    {
        private const int MIN_TEMPERATURE = 1;
        private const int MAX_TEMPERATURE = 40;

        private static double GetTemperature(double temperature, int min, int max)
        {
            var random = new Random();
            double sign = Convert.ToInt32(random.NextDouble() * 2 - 1);
            double increment = sign * random.NextDouble();
            temperature = Math.Min(Math.Max(min, temperature + increment), max);
            return temperature;
        }

        static async Task Main(string[] args)
        {
            var jsonOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            };
            
            var daprClient = new DaprClientBuilder()
            .UseJsonSerializationOptions(jsonOptions)
              .Build();

            var random = new Random();
            var temperature = random.NextDouble() * MAX_TEMPERATURE;

            while (true)
            {
                temperature = GetTemperature(temperature, MIN_TEMPERATURE, MAX_TEMPERATURE);
                var @event = new
                {
                    EventTime = DateTime.UtcNow,
                    TemperatureInCelsius = temperature
                };

                await daprClient.PublishEventAsync("sensors", "temperature", @event);
                Console.WriteLine($"Published event {@event}, sleeping for 5 seconds.");

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}
