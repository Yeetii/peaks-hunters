using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using IO.Swagger.Api;
using IO.Swagger.Client;
using System.Net.Http;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Linq;
using System.Globalization;
using BlazorApp.Shared;


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
            
            Task<List<Shared.Activity>> activitiesTask = Task<List<Shared.Activity>>.Factory.StartNew(() => FetchAllActivities(req.Query["access_token"], log));
            List<Shared.Activity> activities = await activitiesTask;

            List<Shared.Activity> activitiesWithSummits = await MapPeaksToActivities(activities, log);
            Dictionary<string, List<Shared.Activity>> summitedPeaks = ConstructSummitedPeaksDict(activitiesWithSummits);
            return new OkObjectResult(summitedPeaks);
        }

        private static List<Shared.Activity> FetchAllActivities(string AccessToken, ILogger log){
            Configuration.Default.AccessToken = AccessToken;
            var apiInstance = new ActivitiesApi();

            List<Shared.Activity> activities = new List<Shared.Activity>();
            int page = 1;
            while (true){
                List<IO.Swagger.Model.SummaryActivity> results = apiInstance.GetLoggedInAthleteActivities(page: page, perPage: 200);

                foreach (IO.Swagger.Model.SummaryActivity result in results) {
                    try {
                        activities.Add(new Shared.Activity(result));
                    } catch {
                        log.LogError("Activity id: " + result.Id + " could not be parsed!");
                    }
                }

                if (results.Count < 200) {
                    break;
                }
                page++;
            }
            return activities;
        }

        private async static Task<List<Shared.Activity>> MapPeaksToActivities(List<Shared.Activity> activities, ILogger log){
            List<Task<Shared.Activity>> mapPeaksTasks = new List<Task<Activity>>();
            foreach (Shared.Activity activity in activities){
                string polyline = activity.polyline ?? activity.summary_polyline;
                if (String.IsNullOrEmpty(polyline)){
                    continue;
                }
                mapPeaksTasks.Add(MapPeaksToActivity(activity, log));
            }
            
            List<Shared.Activity> activitiesWithSummits = new List<Activity>();
            while (mapPeaksTasks.Any()){
                Task<Shared.Activity> finishedTask = await Task.WhenAny(mapPeaksTasks);
                mapPeaksTasks.Remove(finishedTask);
                Shared.Activity activity = await finishedTask;
                if (activity.peaks is not null && activity.peaks.Count > 0){
                    activitiesWithSummits.Add(activity);
                }
            }
            return activitiesWithSummits;
        }

        private static async Task<Shared.Activity> MapPeaksToActivity(Shared.Activity activity, ILogger log){
            try {
                List<float?> startLatLng = activity.start_latlng;
                float? startLat = startLatLng[0];
                float? startLng = startLatLng[1];
                if (activity.distance.HasValue && startLat.HasValue && startLng.HasValue){
                    string fetchRadius = activity.distance.Value.ToString(CultureInfo.InvariantCulture);
                    string lat = startLat.Value.ToString(CultureInfo.InvariantCulture);
                    string lng = startLng.Value.ToString(CultureInfo.InvariantCulture);
                    string polyline = activity.polyline ?? activity.summary_polyline;
                    Shared.Peak[] allSurroundingPeaks = await HttpClientJsonExtensions.GetFromJsonAsync<Shared.Peak[]>(client, $"/api/Peaks?lat={lat}&lon={lng}&radius={fetchRadius}");
                    
                    List<Shared.Peak> peaks = GeoSpatialFunctions.FindPeaks(allSurroundingPeaks, polyline);
                    List<Shared.PeakInfo> peakInfos = peaks.Select(p => new Shared.PeakInfo(p.id + "", p.name)).ToList();
                    
                    activity.peaks = peakInfos;
                } else {
                    log.LogError("Activity id: " + activity.id + " did not have start_latlng or a distance.");
                }
                } catch (Exception e){
                    log.LogError("Activity id: " + activity.id + " could not be mapped to peaks!");
                    log.LogError(e.ToString());
                }
            return activity;
        }

        private static Dictionary<string, List<Shared.Activity>> ConstructSummitedPeaksDict(List<Shared.Activity> activities){
            Dictionary<string, List<Shared.Activity>> summitedPeaks = new Dictionary<string, List<Shared.Activity>>();

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