using System.Reflection.Metadata;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;


namespace BlazorApp.Api
{
    public class Authenticate
    {
        static HttpClient client = new HttpClient();
        [FunctionName("Authenticate")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string code = req.Query["auth_code"];
            try {
                string clientSecret = Environment.GetEnvironmentVariable($"StravaClientSecret", EnvironmentVariableTarget.Process);
                HttpResponseMessage result = await client.PostAsync("https://www.strava.com/oauth/token?client_id=26280&client_secret=" + clientSecret + "&code=" + code + "&grant_type=authorization_code", null);
                string resultString = await result.Content.ReadAsStringAsync();
                return new OkObjectResult(resultString);
            } catch (Exception e){
                log.LogError(e, "Could not complete authentication");
                return new NotFoundObjectResult(e);
            }
        }
    }
}