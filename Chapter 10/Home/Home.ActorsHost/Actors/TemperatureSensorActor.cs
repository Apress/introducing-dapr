using Dapr.Actors.Runtime;
using Home.Actors;
using System;
using System.Threading.Tasks;

namespace Home.ActorsHost.Actors
{
    public class TemperatureSensorActor : Actor, ITemperatureSensorActor
    {
        private const string STATE_NAME = "sensor_data";
        private const int MIN_TEMPERATURE = 1;
        private const int MAX_TEMPERATURE = 40;

        public TemperatureSensorActor(ActorHost host) : base(host)
        {
        }

        public async Task<double> Measure()
        {
            var currentState = await StateManager.TryGetStateAsync<double>(STATE_NAME);
            var currentTemperature = 0d;
            if (currentState.HasValue)
            {
                currentTemperature = GetNextTemperature(currentState.Value, MIN_TEMPERATURE, MAX_TEMPERATURE);
            }
            else
            {
                var random = new Random();
                currentTemperature = random.NextDouble() * MAX_TEMPERATURE;
            }

            await StateManager.SetStateAsync<double>(STATE_NAME, currentTemperature);

            return currentTemperature;
        }

        private static double GetNextTemperature(double temperature, int min, int max)
        {
            var random = new Random();
            double sign = Convert.ToInt32(random.NextDouble() * 2 - 1);
            double increment = sign * random.NextDouble();
            temperature = Math.Min(Math.Max(min, temperature + increment), max);
            return temperature;
        }
    }
}
