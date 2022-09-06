using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace BlazorApp.Api
{
    public class SummitedPeaksFunction
    {
        [FunctionName("SummitedPeaks")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "Data",
                collectionName: "Strava",
                ConnectionStringSetting = "CosmosDbConnectionString",
                SqlQuery = "SELECT * FROM c WHERE (ARRAY_LENGTH(c.activity.peaks) > 0)"
                )]IEnumerable<Shared.CosmosActivity> activities,
            ILogger log)
        {
            Dictionary<string, List<Shared.Activity>> summitedPeaks = new Dictionary<string, List<Shared.Activity>>();

            foreach (Shared.CosmosActivity activity in activities){
                List<Shared.PeakInfo> peaks = activity.activity.peaks;
                foreach (Shared.PeakInfo peak in peaks){
                    if (!summitedPeaks.ContainsKey(peak.id)){
                        summitedPeaks.Add(peak.id, new List<Shared.Activity>());
                    }
                    summitedPeaks[peak.id].Add(activity.activity);
                }
            }
            return new OkObjectResult(summitedPeaks);
        }
    }
}