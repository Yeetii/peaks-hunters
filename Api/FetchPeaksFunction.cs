using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Linq;
using System.Net.Http;
using BlazorApp.Shared;
using System.Globalization;




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
        static HttpClient client = new HttpClient();
        [FunctionName("FetchPeaks")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB(databaseName: "Data", containerName: "Peaks",
            Connection = "PeaksCosmosDbConnectionString"
            )]IAsyncCollector<dynamic> documentsOut,
            ILogger log)
        {
            string peaksQuery = @"node[""natural""=""peak""]";
            Coordinate topCord = new Coordinate(62.50826064422815,10.733642578125);
            Coordinate botCord = new Coordinate(64.3755654235413,15.4742431640625);
            string rawPeaks = await QueryOverpass(peaksQuery, topCord, botCord, log);

            RootPeaks myDeserializedClass = JsonSerializer.Deserialize<RootPeaks>(rawPeaks);
            Shared.Peak[] peaks = myDeserializedClass.Elements.Select(x => 
                new Shared.Peak(x.Id, x.Tags.Ele, x.Tags.Name, x.Tags.NameSma, x.Tags.AltName, new Shared.Point(new double[] {x.Lon, x.Lat}))).ToArray();

            foreach (Shared.Peak peak in peaks){
                await documentsOut.AddAsync(new Shared.CosmosPeak(peak.id+"", DateTimeOffset.Now, peak));
            }
                
            return new OkObjectResult("Added " + peaks.Length + " peaks to the database");
        }
        private static async Task<string> QueryOverpass(string query, Coordinate topCorner, Coordinate botCorner, ILogger log){
            string bbox = topCorner.lat.ToString(CultureInfo.InvariantCulture) + "," + topCorner.lng.ToString(CultureInfo.InvariantCulture) + "," + 
                botCorner.lat.ToString(CultureInfo.InvariantCulture) + "," + botCorner.lng.ToString(CultureInfo.InvariantCulture);;
            var body = @"[out:json][timeout:25];" + "\n" +
                      $@"            {query}({bbox});" + "\n" +
                       @"            out body;" + "\n" +
                       @"            >;" + "\n" +
                       @"            out skel qt;";
            var buffer = System.Text.Encoding.UTF8.GetBytes(body);
            var byteContent = new ByteArrayContent(buffer);
            var response = await client.PostAsync("https://overpass-api.de/api/interpreter", byteContent);
            string rawResponseString = await response.Content.ReadAsStringAsync();
            return rawResponseString;
        }
    }

}
