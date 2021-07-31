using Dapr.Actors;
using System.Threading.Tasks;

namespace Home.Actors
{
    public interface ITemperatureSensorActor : IActor
    {
        Task<double> Measure();
    }
}
