using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Strava.Api;
using Strava.Client;
using Strava.Model;
using System.Net.Http;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Linq;



namespace BlazorApp.Api
{
    public static class SummitedPeaksFunction
    {
        static HttpClient client = new HttpClient();

        [FunctionName("SummitedPeaks")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            if (client.BaseAddress == null){
                if (Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") == "Development"){
                    client.BaseAddress = new Uri("http://localhost:7071");
                } else {
                    client.BaseAddress = new Uri("https://thankful-ground-0e9ac7c03.1.azurestaticapps.net/");
                }
            }
            
            Task<List<Shared.Activity>> activitiesTask = Task<List<Shared.Activity>>.Factory.StartNew(() => FetchAllActivities(req.Query["access_token"]));
            Task<Shared.Peak[]> peaksTask = HttpClientJsonExtensions.GetFromJsonAsync<Shared.Peak[]>(client, "/api/Peaks");

            Shared.Peak[] allPeaks = await peaksTask ?? new Shared.Peak[]{};
            List<Shared.Activity> activities = await activitiesTask;

            activities = MapPeaksToActivities(activities, allPeaks);
            Dictionary<string, List<Shared.Activity>> summitedPeaks = ConstructSummitedPeaksDict(activities, allPeaks);
            return new OkObjectResult(summitedPeaks);
        }

        private static List<Shared.Activity> FetchAllActivities(string AccessToken){
            Configuration.Default.AccessToken = AccessToken;
            var apiInstance = new ActivitiesApi();

            List<Shared.Activity> activities = new List<Shared.Activity>();
            int page = 1;
            while (true){
                List<Strava.Model.SummaryActivity> results = apiInstance.GetLoggedInAthleteActivities(page: page, perPage: 200);
                
                foreach (Strava.Model.SummaryActivity result in results) {
                    activities.Add(new Shared.Activity(result));
                }

                if (results.Count < 200) {
                    break;
                }
                page++;
            }
            return activities;
        }

        private static List<Shared.Activity> MapPeaksToActivities(List<Shared.Activity> activities, Shared.Peak[] allPeaks){
            foreach (Shared.Activity activity in activities){
                string polyline = activity.polyline ?? activity.summary_polyline;
                if (String.IsNullOrEmpty(polyline)){
                    continue;
                }
                
                List<Shared.Peak> peaks = GeoSpatialFunctions.FindPeaks(allPeaks, polyline);
                List<Shared.PeakInfo> peakInfos = peaks.Select(p => new Shared.PeakInfo(p.id + "", p.name)).ToList();
                
                activity.peaks = peakInfos;
            }
            return activities;
        }


        private static Dictionary<string, List<Shared.Activity>> ConstructSummitedPeaksDict(List<Shared.Activity> activities, Shared.Peak[] allPeaks){
            Dictionary<string, List<Shared.Activity>> summitedPeaks = new Dictionary<string, List<Shared.Activity>>();
            activities = activities.FindAll(a => a.peaks != null && a.peaks.Count > 0);

            foreach (Shared.Activity activity in activities){
                List<Shared.PeakInfo> peaks = activity.peaks;
                foreach (Shared.PeakInfo peak in peaks){
                    if (!summitedPeaks.ContainsKey(peak.id)){
                        summitedPeaks.Add(peak.id, new List<Shared.Activity>());
                    }
                    summitedPeaks[peak.id].Add(activity);
                }
            }
            return summitedPeaks;
        }
    }
}