using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;


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
            string code = req.Query["code"];
            var secretClient = new SecretClient(vaultUri: new Uri("https://pd-key-vault.vault.azure.net/"), credential: new DefaultAzureCredential());
            KeyVaultSecret clientSecretSecret = secretClient.GetSecret("strava-client-secret");
            HttpResponseMessage result = await client.PostAsync("https://www.strava.com/oauth/token?client_id=26280&client_secret=" + clientSecretSecret.Value + "&code=" + code + "&grant_type=authorization_code", null);
            string resultString = await result.Content.ReadAsStringAsync();
            return new OkObjectResult(resultString);
        }
    }
}