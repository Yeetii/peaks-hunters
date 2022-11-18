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
            // Build list of activity fetching tasks
            List<Task<List<Activity>>> mapPeaksToActivitiesTasks = new List<Task<List<Activity>>>();
            bool emptyFetchTask = false;
            int startPage = 1;
            int pagesPerRound = 10;
            while (!emptyFetchTask){
                List<Task<List<IO.Swagger.Model.SummaryActivity>>> activityFetchTasks = StartActivityFetchTasks(req.Query["access_token"], startPage, pagesPerRound);
                while (activityFetchTasks.Any()){
                    Task<List<IO.Swagger.Model.SummaryActivity>> finishedTask = await Task.WhenAny(activityFetchTasks);
                    activityFetchTasks.Remove(finishedTask);
                    List<IO.Swagger.Model.SummaryActivity> results = await finishedTask;
                    if (results.Count > 0){
                        List<Activity> activities = ParseActivities(results, log);
                        mapPeaksToActivitiesTasks.Add(MapPeaksToActivities(activities, log));
                    } else {
                        emptyFetchTask = true;
                    }
                }
                startPage += pagesPerRound;
            }
            // Wait for activity fetching tasks to finish, spin off MapPeaksToActivities on them as they finish
            List<Activity> activitiesWithSummits = new List<Activity>();
            while (mapPeaksToActivitiesTasks.Any()){
                Task<List<Activity>> finishedTask = await Task.WhenAny(mapPeaksToActivitiesTasks);
                mapPeaksToActivitiesTasks.Remove(finishedTask);
                activitiesWithSummits = activitiesWithSummits.Concat(await finishedTask).ToList();
            }

            log.LogInformation("Activities with summits " + activitiesWithSummits.Count);
            
            Dictionary<string, List<Shared.Activity>> summitedPeaks = ConstructSummitedPeaksDict(activitiesWithSummits);
            // Combine results
            return new OkObjectResult(summitedPeaks);
        }

        // Start *pagesPerRound* tasks, if none return empty start another round
        private static List<Task<List<IO.Swagger.Model.SummaryActivity>>> StartActivityFetchTasks(string AccessToken, int startPage, int pagesPerRound){
            Configuration.Default.AccessToken = AccessToken;
            var apiInstance = new ActivitiesApi();
            List<Task<List<IO.Swagger.Model.SummaryActivity>>> fetchTasks = new List<Task<List<IO.Swagger.Model.SummaryActivity>>>();

            for (int page = startPage; page < startPage + pagesPerRound; page++){
                Task<List<IO.Swagger.Model.SummaryActivity>> fetchTask = apiInstance.GetLoggedInAthleteActivitiesAsync(page: page, perPage: 200);
                fetchTasks.Add(fetchTask);
            }
            return fetchTasks;
        }

        private static List<Shared.Activity> ParseActivities(List<IO.Swagger.Model.SummaryActivity> swaggerActivities, ILogger log){
            List<Shared.Activity> activities = new List<Shared.Activity>();
            foreach (IO.Swagger.Model.SummaryActivity result in swaggerActivities) {
                try {
                    activities.Add(new Shared.Activity(result));
                } catch {
                    log.LogError("Activity id: " + result.Id + " could not be parsed!");
                }
            }
            return activities;
        }

        private async static Task<List<Activity>> MapPeaksToActivities(List<Activity> activities, ILogger log){
            List<Task<Activity>> mapPeaksTasks = new List<Task<Activity>>();
            foreach (Activity activity in activities){
                string polyline = activity.polyline ?? activity.summary_polyline;
                if (String.IsNullOrEmpty(polyline)){
                    continue;
                }
                mapPeaksTasks.Add(MapPeaksToActivity(activity, log));
            }

            List<Activity> activitiesWithSummits = new List<Activity>();
            while (mapPeaksTasks.Any()){
                Task<Activity> finishedTask = await Task.WhenAny(mapPeaksTasks);
                mapPeaksTasks.Remove(finishedTask);
                Activity activity = await finishedTask;
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