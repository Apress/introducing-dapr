using Dapr.Actors;
using Dapr.Actors.Client;
using Home.Actors;
using System;
using System.Threading.Tasks;

namespace Home.Person
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var actorId = new ActorId("LivingRoom");
            var roomProxy = ActorProxy.Create<IRoomActor>(actorId, "RoomActor");
            await roomProxy.TurnOnLights();

            var airConId = await roomProxy.GetAirConActorId();
            var airConProxy = ActorProxy.Create<IAirConActor>(airConId, "AirConActor");
            await airConProxy.TurnOn();
            await airConProxy.SetMode(AirConMode.Cool);
            await airConProxy.SetTargetTemperature(24d);

            for (int i = 0; i < 20; i++)
            {
                Console.WriteLine(await roomProxy.Describe());
                await Task.Delay(15000);
            }

            await airConProxy.TurnOff();
            Console.WriteLine(await roomProxy.Describe());
        }
    }
}
