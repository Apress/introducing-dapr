using Dapr.Actors;
using System.Threading.Tasks;

namespace Home.Actors
{
    public interface IRoomActor : IActor
    {
        Task TurnOnLights();
        Task TurnOffLights();
        Task<ActorId> GetAirConActorId();
        Task<string> Describe();
    }
}
