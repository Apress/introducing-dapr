using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CloudNative.CloudEvents;
using Newtonsoft.Json.Linq;

namespace ACController.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TemperatureMeasurementController : ControllerBase
    {
        private readonly ILogger<TemperatureMeasurementController> _logger;

        public TemperatureMeasurementController(ILogger<TemperatureMeasurementController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post([FromBody] CloudEvent cloudEvent)
        {
            dynamic data = cloudEvent.Data;
            var temperature = data.temperatureInCelsius;
            var airCon = AirConditioner.Instance;

            Console.WriteLine($"Temperature is {temperature} degree Celsius.");
            if (temperature < 21 || temperature > 23)
            {
                airCon.TurnOn();
                if (temperature < 21)
                {
                    airCon.SetMode(AirConditionerMode.Heat);
                }
                else
                {
                    airCon.SetMode(AirConditionerMode.Cool);
                }
            }
            else
            {
                airCon.TurnOff();
            }

            return Ok();
        }
    }
}
