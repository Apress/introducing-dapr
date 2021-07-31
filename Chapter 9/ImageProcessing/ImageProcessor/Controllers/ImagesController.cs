using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ObjectRecognition;
using ObjectRecognition.DataStructures;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessor.Controllers
{
    [ApiController]
    [Route("azqueue")]
    public class ImagesController : ControllerBase
    {
        private readonly ILogger<ImagesController> _logger;
        private readonly IConfiguration _configuration;
        private readonly int _daprPort;
        private readonly string _storageBindingName = "azblob";
        private readonly string _stateStoreName = "statestore";

        public ImagesController(ILogger<ImagesController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            var daprPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT");
            _daprPort = daprPort != null ? int.Parse(daprPort) : 3500;
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync()
        {
            _logger.LogInformation("Input binding endpoint was triggered.");
            using var stream = new StreamReader(HttpContext.Request.Body);
            var bodyString = await stream.ReadToEndAsync();
            var converter = new ExpandoObjectConverter();
            dynamic body = JsonConvert.DeserializeObject<ExpandoObject>(bodyString, converter);
            string photoUrl = body.data.url;
            _logger.LogInformation($"An image with the following url was uploaded: {photoUrl}.");

            var containerPath = new Uri(photoUrl);
            var imagePathInContainer = string.Join('/', containerPath.LocalPath.Split('/')
                .Where(x => x != string.Empty).Skip(1));
            var tempImagePath = Path.GetTempFileName();
            _logger.LogInformation($"Downloading the image to {tempImagePath}.");
            await DownloadFileAsync(imagePathInContainer, tempImagePath);

            _logger.LogInformation("Model inference started.");
            var tempTaggedImagePath = Path.GetTempFileName();
            var modelEvaluator = new ModelEvaluator();
            var results = modelEvaluator.Evaluate(tempImagePath, tempTaggedImagePath);
            _logger.LogInformation($"Model inference finished. The tagged image is: {tempTaggedImagePath}.");

            var imageUploadPath = imagePathInContainer.Replace("input/", "output/");
            _logger.LogInformation($"Uploading the image back to Azure Storage at the following location inside the container: {imageUploadPath}.");
            await UploadFileAsync(tempTaggedImagePath, imageUploadPath);

            var objectInfo = string.Join(Environment.NewLine, results.GroupBy(x => x.Label)
                            .Select(x => $"{x.Key}: {x.Count()} times"));
            _logger.LogInformation($"The following objects were recognized:{Environment.NewLine}{objectInfo}");
            return Ok();
        }

        private async Task DownloadFileAsync(string pathInContainer, string targetDownloadPath)
        {
            var outpuBindingData = new
            {
                operation = "get",
                metadata = new
                {
                    blobName = pathInContainer
                }
            };
            var json = JsonConvert.SerializeObject(outpuBindingData);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            var httpClient = new HttpClient();
            var response = await httpClient.PostAsync($"http://localhost:{_daprPort}/v1.0/bindings/{_storageBindingName}", stringContent);

            using (var fs = new FileStream(targetDownloadPath, FileMode.OpenOrCreate))
            {
                await response.Content.CopyToAsync(fs);
            }
        }

        private async Task UploadFileAsync(string targetImageTempPath, string targetUploadPath)
        {
            string base64Image = await FileToBase64Async(targetImageTempPath);
            var outputBindingData = new
            {
                operation = "create",
                data = base64Image,
                metadata = new
                {
                    blobName = targetUploadPath
                }
            };
            var json = JsonConvert.SerializeObject(outputBindingData);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync($"http://localhost:{_daprPort}/v1.0/bindings/{_storageBindingName}", stringContent);
        }

        private static async Task<string> FileToBase64Async(string filePath)
        {
            using (var outputFile = new FileStream(filePath, FileMode.Open))
            using (var memoryStream = new MemoryStream())
            {
                await outputFile.CopyToAsync(memoryStream);
                var base64Content = Convert.ToBase64String(memoryStream.ToArray());
                return base64Content;
            }
        }
    }
}