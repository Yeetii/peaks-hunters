using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Azure.Documents.Client;
using System.Linq;
using System;
using System.Data;
using Microsoft.Azure.Documents.Linq;


namespace BlazorApp.PeaksFunction
{
    public static class PeaksFunction
    {
        [FunctionName("Peaks")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "Data",
                collectionName: "Peaks",
                ConnectionStringSetting = "CosmosDbConnectionString")]DocumentClient documentClient,
            ILogger log)
        {


            List<BlazorApp.Shared.CosmosPeak> cosmosPeaks = await FetchWholeCollection<BlazorApp.Shared.CosmosPeak>("Peaks", documentClient);
            BlazorApp.Shared.Peak[] allPeaks = cosmosPeaks.Select(x => x.peak).ToArray();
            return new OkObjectResult(allPeaks);
            
        }
        public static async Task<List<T>> FetchWholeCollection<T>(string collectionName, DocumentClient client){
            var option = new FeedOptions { EnableCrossPartitionQuery = true };
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("Data", collectionName);
            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(collectionUri, option)
                .Where(p => true)
                .AsDocumentQuery();

            List<T> allDocuments = new List<T>();
            while (query.HasMoreResults)
            {
                foreach (T document in await query.ExecuteNextAsync())
                {
                    allDocuments.Add(document);
                }
            }
            return allDocuments;
        }
    }
}
