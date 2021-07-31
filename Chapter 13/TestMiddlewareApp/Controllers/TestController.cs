using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TestMiddlewareApp.Controllers
{
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [Route("/test")]
        [HttpGet, HttpPut, HttpPost, HttpDelete]
        public async Task<string> Test()
        {
            var response = $"HTTP Method: {Request.Method}\n" +
                $"Query: {Request.QueryString} \n" +
                "Headers\n";
            foreach (var header in Request.Headers)
            {
                response += $"\t{header.Key}: {header.Value}\n";
            }

            using var reader = new StreamReader(Request.Body);
            response += $"Body: {await reader.ReadToEndAsync()}";

            return response;
        }
    }
}
