using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace GFMS_Monkey.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebHookController : ControllerBase
    {

        private string _baseUrl = "https://gfms-sandbox-monkeyisland.azurewebsites.net/";
        private string _callbackUrl = "https://gfms-monkey.northeurope.cloudapp.azure.com/WebHook";

        private class MonkeyIslandResponse
        {
            public int[]? magicNumbers { get; set; }
        }

        private class MonkeyIslandRequest
        {
            public int sum { get; set; }
            public string? callBackUrl { get; set; }
        }

        private class WebHookResponse
        {
            public bool success { get; set; } = true;
            public string message { get; set; } = "";
        }

        private readonly ILogger<WebHookController> _logger;

        public WebHookController(ILogger<WebHookController> logger)
        {
            _logger = logger;
            _baseUrl += Environment.GetEnvironmentVariable("API_KEY");
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            // perform http request to the GFMS API
            var client = new HttpClient();
            var response = await client.GetAsync(_baseUrl);
            var content = await response.Content.ReadAsStringAsync();

            // parse JSON
            MonkeyIslandResponse r = JsonConvert.DeserializeObject<MonkeyIslandResponse>(content);

            // calc sum
            int sum = r.magicNumbers.Sum();

            // send it back as json to GFMS
            var json = JsonConvert.SerializeObject(new MonkeyIslandRequest
            {
                sum = sum,
                callBackUrl = _callbackUrl
            });

            await client.PostAsync(
                _baseUrl,
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            // return the magic numbers
            return Ok(new WebHookResponse
            {
                success = true,
                message = "Request sent."
            });

        }

        [HttpPost]
        public async Task<IActionResult> PostAsync()
        {
            var request = await new StreamReader(Request.Body).ReadToEndAsync();
            _logger.LogInformation(request);
            return Ok();
        }
    }
}