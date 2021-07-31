using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Actors.Runtime;
using Home.Actors;
using System.Text;
using System.Threading.Tasks;

namespace Home.ActorsHost.Actors
{
    public class RoomActor : Actor, IRoomActor
    {
        private const string STATE_NAME = "room_data";

        public RoomActor(ActorHost host) : base(host)
        {
        }

        private async Task SetLights(bool state)
        {
            var data = await StateManager.GetStateAsync<RoomData>(STATE_NAME);
            data.AreLightsOn = state;
            await StateManager.SetStateAsync(STATE_NAME, data);
        }

        protected override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();

            var stateExists = await StateManager.ContainsStateAsync(STATE_NAME);
            if (!stateExists)
            {
                var data = new RoomData()
                {
                    AreLightsOn = false,
                    AirConActorId = ActorId.CreateRandom()
                };
                await StateManager.SetStateAsync(STATE_NAME, data);
            }
        }

        public async Task TurnOffLights()
        {
            await SetLights(false);
        }

        public async Task TurnOnLights()
        {
            await SetLights(true);
        }

        public async Task<ActorId> GetAirConActorId()
        {
            var data = await StateManager.GetStateAsync<RoomData>(STATE_NAME);
            return data.AirConActorId;
        }

        public async Task<string> Describe()
        {
            var data = await StateManager.GetStateAsync<RoomData>(STATE_NAME);
            var airConId = await GetAirConActorId();
            var airConProxy = ActorProxy.Create<IAirConActor>(airConId, "AirConActor");
            var currentTemperature = await airConProxy.GetCurrentTemperature();

            var descriptionSb = new StringBuilder();
            descriptionSb.Append($"The current room temperature is {currentTemperature:0.#}. ");
            if (await airConProxy.IsTurnedOn())
            {
                descriptionSb.Append($"The AC is turned on and set to {await airConProxy.GetMode()} mode. ");
                var currentState = await airConProxy.GetState();
                var targetTemperature = await airConProxy.GetTargetTemperature();
                if (currentState == AirConState.Working)
                {
                    descriptionSb.Append($"It is working hard to achieve the target temperature of {targetTemperature:0.#} degrees Celsius. ");
                }
                else
                {
                    descriptionSb.Append($"It is currently idle as the the target temperature of {targetTemperature:0.#} degrees Celsius has been reached. ");
                }
            }
            else
            {
                descriptionSb.Append($"The AC is turned off. ");
            }

            descriptionSb.Append("The lights in the room are: " + (data.AreLightsOn ? "On. " : "Off. "));

            return descriptionSb.ToString();
        }
    }

    public class RoomData
    {
        public bool AreLightsOn { get; set; }
        public ActorId AirConActorId { get; set; }
    }
}
