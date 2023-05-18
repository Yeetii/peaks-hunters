using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Cosmos;

namespace BlazorApp.Api
{
    public static class PeaksFunction
    {
        [FunctionName("Peaks")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "Data",
                containerName: "Peaks",
                Connection = "PeaksCosmosDbConnectionString")]CosmosClient client)
        {
            List<BlazorApp.Shared.CosmosPeak> cosmosPeaks;

            string lat = req.Query["lat"].ToString();
            string lon = req.Query["lon"].ToString();
            const int defaultRadius = 40000;
            if (!int.TryParse(req.Query["radius"], out int radius)){
                radius = defaultRadius;
            }

            if (!(string.IsNullOrEmpty(lat) || string.IsNullOrEmpty(lon))){
                cosmosPeaks = await GeoSpatialFetch<Shared.CosmosPeak>(client, lat, lon, radius);
            } else {
                cosmosPeaks = await FetchWholeCollection<Shared.CosmosPeak>(client);
            }

            Shared.Peak[] peaks = cosmosPeaks.Select(x => x.peak).ToArray();
            return new OkObjectResult(peaks);
        }

        public static async Task<List<T>> GeoSpatialFetch<T>(CosmosClient client, string lat, string lon, int radius){
            string query = string.Join(Environment.NewLine,
            "SELECT *",
            "FROM p",
            $"WHERE ST_DISTANCE(p.peak.location, {{'type': 'Point', 'coordinates':[{lon}, {lat}]}}) < {radius}");

            return await QueryCollection<T>(client, query);
        }

        public static async Task<List<T>> FetchWholeCollection<T>(CosmosClient client){
            return await QueryCollection<T>(client, "SELECT * FROM p");
        }

        public static async Task<List<T>> QueryCollection<T>(CosmosClient client, string query){
            List<T> documents = new();

            Container container = client.GetDatabase("Data").GetContainer("Peaks");
            QueryDefinition queryDefinition = new(query);
            using (FeedIterator<T> resultSet = container.GetItemQueryIterator<T>(queryDefinition))
            {
                while (resultSet.HasMoreResults)
                {
                    FeedResponse<T> response = await resultSet.ReadNextAsync();
                    documents.AddRange(response);
                }
            }
            return documents;
        }
    }
}
