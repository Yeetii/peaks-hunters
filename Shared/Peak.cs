
namespace BlazorApp.Shared
{
    // Special format needed for CosmosDB spatial queries https://docs.microsoft.com/en-us/azure/cosmos-db/sql/sql-query-geospatial-intro
    public class Peak {
        public Peak(){}

        public Peak(long id, string elevation, string name, string name_sapmi, string name_alt, Point location){
            this.id = id;
            this.elevation = elevation;
            this.name = name;
            this.name_sapmi = name_sapmi;
            this.name_alt = name_alt;
            this.location = location;
        }

        public long id {get; set;}
        public string elevation {get; set;}
        public string name {get; set;}
        public string name_sapmi {get; set;}
        public string name_alt {get; set;}
        public Point location {get; set;}
    }

    public class Point {
        public double[] coordinates {get; set;}
        public string type {get; set;}
        public Point(double[] coordinates){
            this.coordinates = coordinates;
            type = "Point";
        }
    }
}