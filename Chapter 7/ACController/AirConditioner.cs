using System;

namespace ACController
{
    public class AirConditioner
    {
        public bool IsTurnedOn { get; private set; }
        public AirConditionerMode? Mode { get; private set; }

        public static AirConditioner Instance { get; } = new AirConditioner();

        private AirConditioner()
        {
            IsTurnedOn = false;
            SetMode(AirConditionerMode.Cool);
        }

        public void SetMode(AirConditionerMode mode)
        {
            if (Mode != mode)
            {
                Mode = mode;
                Console.WriteLine($"The AC is set to {Mode.ToString()} mode.");
            }
        }

        public void TurnOn()
        {
            if (!IsTurnedOn)
            {
                IsTurnedOn = true;
                Console.WriteLine("The AC is turned on.");
            }
        }

        public void TurnOff()
        {
            if (IsTurnedOn)
            {
                IsTurnedOn = false;
                Console.WriteLine("The AC is turned off.");
            }
        }
    }

    public enum AirConditionerMode
    {
        Heat, Cool
    }
}