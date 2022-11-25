using System.Data;
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
        private const string devApiUri = "http://localhost:7071";
        private const string prodApiUri = "https://thankful-ground-0e9ac7c03.1.azurestaticapps.net/";
        static HttpClient client = new HttpClient();

        [FunctionName("SummitedPeaks")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            if (client.BaseAddress == null){
                if (Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") == "Development"){
                    client.BaseAddress = new Uri(devApiUri);
                } else {
                    client.BaseAddress = new Uri(prodApiUri);
                }
            }
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
            List<Activity> activitiesWithSummits = new List<Activity>();
            while (mapPeaksToActivitiesTasks.Any()){
                Task<List<Activity>> finishedTask = await Task.WhenAny(mapPeaksToActivitiesTasks);
                mapPeaksToActivitiesTasks.Remove(finishedTask);
                List<Activity> newMappedOnes = await finishedTask;
                activitiesWithSummits.AddRange(newMappedOnes);
            }

            log.LogInformation("Activities with summits " + activitiesWithSummits.Count);
            Dictionary<string, List<Shared.Activity>> summitedPeaks = ConstructSummitedPeaksDict(activitiesWithSummits);
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
            (Coordinate coordinate, float radius) = CaclulateContainingCircle(activities);
            string fetchRadius = radius.ToString(CultureInfo.InvariantCulture);
            string lat = coordinate.lat.ToString(CultureInfo.InvariantCulture);
            string lng = coordinate.lng.ToString(CultureInfo.InvariantCulture);
            Peak[] allSurroundingPeaks = await HttpClientJsonExtensions.GetFromJsonAsync<Shared.Peak[]>(client, $"/api/Peaks?lat={lat}&lon={lng}&radius={fetchRadius}");
            log.LogInformation("Mapping activities on " + allSurroundingPeaks.Length + " peaks");

            List<Activity> activitiesWithSummits = new List<Activity>();
            foreach (Activity activity in activities){
                string polyline = activity.polyline ?? activity.summary_polyline;
                if (String.IsNullOrEmpty(polyline)){
                    continue;
                }
                Activity mappedActivity = MapPeaksToActivity(activity, allSurroundingPeaks, log);
                if (mappedActivity.peaks is not null && mappedActivity.peaks.Count > 0){
                    activitiesWithSummits.Add(mappedActivity);
                } 
            }
            return activitiesWithSummits;
        }

        private static Activity MapPeaksToActivity(Shared.Activity activity, Peak[] allSurroundingPeaks, ILogger log){
            try {
                Coordinate startCoordinate = Coordinate.ParseCoordinate(activity.start_latlng);
                if (activity.distance.HasValue && startCoordinate is not null){
                    string polyline = activity.polyline ?? activity.summary_polyline;
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

        // Roughly calculate a circle that contains the entirity of all activities
        // Problematically large circle for activities that are far apart
        private static (Coordinate, float) CaclulateContainingCircle(List<Activity> activities){
            Coordinate coordinate = null;
            float maxDistance = 0;
            float maxActivityDistance = 0;
            foreach (Activity activity in activities){
                if (coordinate is null){
                    coordinate = Coordinate.ParseCoordinate(activity.start_latlng);
                }
                if ((activity.distance ?? 0) > maxActivityDistance){
                    maxActivityDistance = activity.distance.Value;
                }
                Coordinate activityStartPos = Coordinate.ParseCoordinate(activity.start_latlng);
                if (activityStartPos is not null && coordinate is not null){
                    float distance = (float) GeoSpatialFunctions.DistanceTo(coordinate, activityStartPos);
                    if (distance > maxDistance){
                        maxDistance = distance;
                    }
                }
            }
            return (coordinate, maxActivityDistance + maxDistance);
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