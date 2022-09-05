
namespace BlazorApp.Shared
{

    // Special format needed for CosmosDB spatial queries https://docs.microsoft.com/en-us/azure/cosmos-db/sql/sql-query-geospatial-intro
    public class Peak {
        public Peak(){}

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