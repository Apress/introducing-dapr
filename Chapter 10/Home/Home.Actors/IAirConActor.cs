using Dapr.Actors;
using System.Threading.Tasks;

namespace Home.Actors
{
    public interface IAirConActor : IActor
    {
        Task TurnOn();
        Task TurnOff();
        Task<bool> IsTurnedOn();
        Task SetTargetTemperature(double temperature);
        Task<double> GetTargetTemperature();
        Task<double> GetCurrentTemperature();
        Task SetMode(AirConMode mode);
        Task<AirConMode> GetMode();
        Task<AirConState> GetState();
    }

    public enum AirConMode
    {
        Heat, Cool
    }

    public enum AirConState
    {
        Idle, Working
    }
}
