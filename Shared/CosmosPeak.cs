using System;

namespace BlazorApp.Shared
{
    public class CosmosPeak
    {
        public CosmosPeak(string id, DateTimeOffset fetchDate, Peak peak){
            this.id = id;
            fetch_date = fetchDate;
            this.peak = peak;
        }

        public string id {get; set;}
        public DateTimeOffset fetch_date {get; set;}
        public Peak peak {get; set;}
    }
}