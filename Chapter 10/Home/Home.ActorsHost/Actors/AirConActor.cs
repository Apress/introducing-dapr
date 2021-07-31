using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Actors.Runtime;
using Home.Actors;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Home.ActorsHost.Actors
{
    public class AirConActor : Actor, IAirConActor, IRemindable
    {
        private const string STATE_NAME = "aircon_data";

        public AirConActor(ActorHost host) : base(host)
        {
        }

        protected override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();

            var stateExists = await StateManager.ContainsStateAsync(STATE_NAME);
            if (!stateExists)
            {
                var data = new AirConData()
                {
                    TemperatureSensorActorId = ActorId.CreateRandom(),
                    IsTurnedOn = false,
                    Mode = AirConMode.Cool,
                    TargetTemperature = 20d
                };
                await StateManager.SetStateAsync(STATE_NAME, data);
            }
        }

        public async Task<AirConMode> GetMode()
        {
            var state = await StateManager.GetStateAsync<AirConData>(STATE_NAME);
            return state.Mode;
        }

        public async Task SetMode(AirConMode mode)
        {
            var state = await StateManager.GetStateAsync<AirConData>(STATE_NAME);
            state.Mode = mode;
            await StateManager.SetStateAsync(STATE_NAME, state);
        }

        public async Task SetTargetTemperature(double temperature)
        {
            var state = await StateManager.GetStateAsync<AirConData>(STATE_NAME);
            state.TargetTemperature = temperature;
            await StateManager.SetStateAsync(STATE_NAME, state);
        }

        public async Task<double> GetCurrentTemperature()
        {
            var airConState = await StateManager.GetStateAsync<AirConData>(STATE_NAME);
            var sensorProxy = ActorProxy.Create<ITemperatureSensorActor>(airConState.TemperatureSensorActorId, "TemperatureSensorActor");
            var currentTemperature = await sensorProxy.Measure();
            return currentTemperature;
        }

        public async Task TurnOff()
        {
            var state = await StateManager.GetStateAsync<AirConData>(STATE_NAME);
            state.IsTurnedOn = false;
            await StateManager.SetStateAsync(STATE_NAME, state);

            await UnregisterReminderAsync("control-loop");
        }

        public async Task TurnOn()
        {
            var state = await StateManager.GetStateAsync<AirConData>(STATE_NAME);
            state.IsTurnedOn = true;
            await StateManager.SetStateAsync(STATE_NAME, state);

            await RegisterReminderAsync("control-loop", null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));
        }

        public async Task<bool> IsTurnedOn()
        {
            var airConState = await StateManager.GetStateAsync<AirConData>(STATE_NAME);
            return airConState.IsTurnedOn;
        }

        public async Task<double> GetTargetTemperature()
        {
            var airConState = await StateManager.GetStateAsync<AirConData>(STATE_NAME);
            return airConState.TargetTemperature;
        }

        public async Task<AirConState> GetState()
        {
            var airConState = await StateManager.GetStateAsync<AirConData>(STATE_NAME);
            return airConState.State;
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            var airConState = await StateManager.GetStateAsync<AirConData>(STATE_NAME);
            var sensorProxy = ActorProxy.Create<ITemperatureSensorActor>(airConState.TemperatureSensorActorId, "TemperatureSensorActor");
            var currentTemperature = await sensorProxy.Measure();

            Logger.LogInformation($"The current room temperature is {currentTemperature:0.#} and " +
                $"the AC is set to {airConState.TargetTemperature:0.#} in {airConState.Mode} mode.");
            if (currentTemperature < airConState.TargetTemperature)
            {
                airConState.State = (airConState.Mode == AirConMode.Cool) ? AirConState.Idle : AirConState.Working;
                Logger.LogInformation("The temperature is below the set temperature.");
                Logger.LogInformation((airConState.Mode == AirConMode.Cool) ? "Standing by." : "Heating...");
            }
            else
            {
                airConState.State = (airConState.Mode == AirConMode.Cool) ? AirConState.Working : AirConState.Idle;
                Logger.LogInformation("The temperature is above the set temperature.");
                Logger.LogInformation((airConState.Mode == AirConMode.Cool) ? "Cooling..." : "Standing by.");
            }
            await StateManager.SetStateAsync(STATE_NAME, airConState);
        }
    }

    public class AirConData
    {
        public ActorId TemperatureSensorActorId { get; set; }
        public AirConMode Mode { get; set; }
        public AirConState State { get; set; }
        public bool IsTurnedOn { get; set; }
        public double TargetTemperature { get; set; }
    }
}
