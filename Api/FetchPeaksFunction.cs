using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Linq;
using System.Net.Http;

namespace BlazorApp.Api
{
    public class RootPeaks
    {
        [JsonPropertyName("elements")]
        public List<RawPeaks> Elements { get; set; }
    }
        public class RawPeaks
    {
        [JsonPropertyName("id")]
        public Int64 Id { get; set; }

        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lon")]
        public double Lon { get; set; }

        [JsonPropertyName("tags")]
        public Tags Tags { get; set; }
    }
    public class Tags
    {
        [JsonPropertyName("ele")]
        public string Ele { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("name:sma")]
        public string NameSma { get; set; }

        [JsonPropertyName("alt_name")]
        public string AltName { get; set; }
    }

    public static class FetchPeaksFunction
    {
        private const string overpassUri = "https://overpass-api.de/api/interpreter";
        static readonly HttpClient client = new();
        [FunctionName("FetchPeaks")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest _,
            [CosmosDB(databaseName: "Data", containerName: "Peaks",
            Connection = "PeaksCosmosDbConnectionString"
            )]IAsyncCollector<dynamic> documentsOut)
        {
            const string body = @"[out:json][timeout:25];" + "\n" +
            @"            node[""natural""=""peak""](62.50826064422815,10.733642578125,64.3755654235413,15.4742431640625);" + "\n" +
            @"            out body;" + "\n" +
            @"            >;" + "\n" +
            @"            out skel qt;";
            var buffer = System.Text.Encoding.UTF8.GetBytes(body);
            var byteContent = new ByteArrayContent(buffer);
            var response = await client.PostAsync(overpassUri, byteContent);
            string rawPeaks = await response.Content.ReadAsStringAsync();
            RootPeaks myDeserializedClass = JsonSerializer.Deserialize<RootPeaks>(rawPeaks);
            Shared.Peak[] peaks = myDeserializedClass.Elements.Select(x => 
                new Shared.Peak(x.Id, x.Tags.Ele, x.Tags.Name, x.Tags.NameSma, x.Tags.AltName, new Shared.Point(new double[] {x.Lon, x.Lat}))).ToArray();

            foreach (Shared.Peak peak in peaks){
                await documentsOut.AddAsync(new Shared.CosmosPeak(peak.id+"", DateTimeOffset.Now, peak));
            }
            return new OkObjectResult("Added " + peaks.Length + " peaks to the database");
        }
    }
}
