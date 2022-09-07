using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace BlazorApp.Api
{
    public class GeoSpatialFunctions
    {
        static HttpClient client = new HttpClient();
        public static NetTopologySuite.Geometries.GeometryFactory GetGeometryFactory(){
            NetTopologySuite.NtsGeometryServices.Instance = new NetTopologySuite.NtsGeometryServices(
            // default CoordinateSequenceFactory
            NetTopologySuite.Geometries.Implementation.CoordinateArraySequenceFactory.Instance,
            new NetTopologySuite.Geometries.PrecisionModel(1000d),
            // default SRID
            4326,
            // Geometry overlay operation function set to use (Legacy or NG)
            NetTopologySuite.Geometries.GeometryOverlay.NG,
            // Coordinate equality comparer to use (CoordinateEqualityComparer or PerOrdinateEqualityComparer)
            new NetTopologySuite.Geometries.CoordinateEqualityComparer());
            return NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
        }

        // TODO when refactoring make it general, remove dependency on Model.Peak 
        public static List<Shared.Peak> FindPeaks(Shared.Peak[] peaks, string polylineString){
        var gf = GetGeometryFactory();
        IEnumerable<NetTopologySuite.Geometries.Coordinate> polyline = DecodePolyLine(polylineString);
        NetTopologySuite.Geometries.LineString line = gf.CreateLineString(polyline.ToArray());

        List<Shared.Peak> matches = new List<Shared.Peak>();
        foreach (Shared.Peak peak in peaks){
            NetTopologySuite.Geometries.Polygon peakBox = CalculateBoundingBox(
                new NetTopologySuite.Geometries.Coordinate(peak.location.coordinates[0], peak.location.coordinates[1]), 50);
            if (line.Intersects(peakBox)){
                matches.Add(peak);
            }
        }
        return matches;
        }

        // Credit: https://gis.stackexchange.com/questions/15545/calculating-coordinates-of-square-x-miles-from-center-point
        // Returns a box with the given coordinate in center and side length of about 2*radius metres
        public static NetTopologySuite.Geometries.Polygon CalculateBoundingBox(NetTopologySuite.Geometries.Coordinate point, float radius){
            double latitude = point.Y;
            double longitude = point.X;
            double dLat = radius / 111100; // The distance between each latitude is about 111.1 km
            double dLong = dLat / Math.Cos(latitude); // The distance between each longitude depends on the latitude

            var gf = GetGeometryFactory();

            var ply1 = gf.CreatePolygon(new[] {
                new NetTopologySuite.Geometries.Coordinate(longitude-dLong, latitude-dLat),
                new NetTopologySuite.Geometries.Coordinate(longitude-dLong, latitude+dLat),
                new NetTopologySuite.Geometries.Coordinate(longitude+dLong, latitude+dLat),
                new NetTopologySuite.Geometries.Coordinate(longitude+dLong, latitude-dLat),
                new NetTopologySuite.Geometries.Coordinate(longitude-dLong, latitude-dLat),
            });
            return ply1;
        }


        public static double DistanceTo(NetTopologySuite.Geometries.Coordinate p1, NetTopologySuite.Geometries.Coordinate p2)
        {
            double lat1 = p1.Y;
            double lon1 = p1.X;
            double lat2 = p2.Y;
            double lon2 = p2.X;
            double rlat1 = Math.PI*lat1/180;
            double rlat2 = Math.PI*lat2/180;
            double theta = lon1 - lon2;
            double rtheta = Math.PI*theta/180;
            double dist =
                Math.Sin(rlat1)*Math.Sin(rlat2) + Math.Cos(rlat1)*
                Math.Cos(rlat2)*Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist*180/Math.PI;
            return dist*111189.57696; // Result in metres
        }


        // This function was taken from https://gist.github.com/shinyzhu/4617989 and modified a little
        public static IEnumerable<NetTopologySuite.Geometries.Coordinate> DecodePolyLine(string encodedPoints)
        {
            if (string.IsNullOrEmpty(encodedPoints))
                throw new ArgumentNullException("encodedPoints");

            char[] polylineChars = encodedPoints.ToCharArray();
            int index = 0;

            int currentLat = 0;
            int currentLng = 0;
            int next5bits;
            int sum;
            int shifter;

            while (index < polylineChars.Length)
            {
                // calculate next latitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length)
                    break;

                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                //calculate next longitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && next5bits >= 32)
                    break;

                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                yield return new NetTopologySuite.Geometries.Coordinate(Convert.ToDouble(currentLng) / 1E5, Convert.ToDouble(currentLat) / 1E5);
            }
        }
    }
}