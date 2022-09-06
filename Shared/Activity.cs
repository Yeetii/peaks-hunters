using System;
using System.Collections.Generic;
namespace BlazorApp.Shared

{
    public class PeakInfo{
        public string id {get; set;}
        public string name {get; set;}
        public PeakInfo(string id, string name){
            this.id = id;
            this.name = name;
        }
    }
    public class Activity
    {
        public long? id {get; set;}
        public string name {get; set;}
        public string description {get; set;}
        public float? distance {get; set;}
        public float? moving_time {get; set;}
        public float? elapsed_time {get; set;}
        public float? calories {get; set;}
        public float? total_elevation_gain {get; set;}
        public float? elev_high {get; set;}
        public float? elev_low {get; set;}
        public string sport_type {get; set;}
        public System.DateTime? start_date {get; set;}
        public System.DateTime? start_date_local {get; set;}
        public string timezone {get; set;}
        public List<float?> start_latlng {get; set;}
        public List<float?> end_latlng {get; set;}
        public int? athlete_count {get; set;}
        public float? average_speed {get; set;}
        public float? max_speed {get; set;}
        public string polyline {get; set;}
        public string summary_polyline {get; set;}
        public List<PeakInfo> peaks {get; set;}
    }
}