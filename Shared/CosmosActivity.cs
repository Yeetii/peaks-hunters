using System;

namespace BlazorApp.Shared

{
    public class CosmosActivity
    {
        public CosmosActivity(string id, DateTimeOffset fetch_date, bool detailed_activity, Activity activity) {
            this.id = id;
            this.fetch_date = fetch_date;
            this.detailed_activity = detailed_activity;
            this.activity = activity;
        }

        public string id {get; set;}
        public DateTimeOffset fetch_date {get; set;}
        public bool detailed_activity {get; set;}
        public Activity activity {get; set;}
    }
}