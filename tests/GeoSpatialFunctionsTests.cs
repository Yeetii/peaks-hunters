using System.Reflection;
using System.Collections.Generic;
using System.Text.Json;
using BlazorApp.Api;
using BlazorApp.Shared;

namespace tests;

public class GeoSpatialFunctionsTests
{

    [Fact]
    public void DecodePolyLineTest()
    {
        string polyline = "o_}aKcq|nAAASZU`@Yt@OVu@nBSRIo@Me@IKE?GEM?MMMEEG?o@ASGMC@EIAHG@CAMo@ECIc@CYIM?}@KM@WMeA?e@Ci@JqA?oBBY?YFe@AYGMKBCAGICHEEAG?QDWBcAN}@J}BHq@@a@Ji@@MEIB]Ji@FKB?CM?QFa@DKLMDM@g@RaAHg@Bg@Pu@HgBN_AAEGBa@XU@EBICQNKCEIE[Hq@H}ABo@Ac@B{@GiA@k@E]LcA@WFs@?WFYDcBC{ACQBa@VwBt@aEL}ATsADaAJw@DyADW?QHo@Ay@BWDuAFYHw@FYBm@DU@o@BCGsBEMECOACDEAEJKDCFKJe@AMKKEMDOGM@CIYIGEg@u@KIIOSOOEMOOEe@]WWQa@UK[?KCKDMAQKOSWq@[wAISi@k@iA_AUg@QKM?a@OIMWCa@d@[n@Y\\O\\KNWh@GTOfAAn@K|@i@hAq@x@{ArAm@p@g@r@s@p@[d@E^Jj@?dAJPp@^FCNAND^CN@DOFEPADCFWHAF@B?HWP]BCHDLONDJC\\]DDJKH?PFP?n@O^BVHXXb@VR\\JENBvAsFVr@I^Nr@GpBHrCyAVHTBhAISe@n@EXBfBJp@Bj@W~BUT?DETOPEPUHGJMFMGGRYtEE~AGZ[|CE~ADv@AhAGtAIn@ILGV?LDh@Al@QbC@jAIv@Cv@Bn@Hr@I^Iv@DpAGjBBxCDn@Nx@BlBCTYp@BZCTJh@?bAJf@@LCNKRCRGJGRE@IIKHC?CJMKCB@PTr@JPGHQISBEASw@GKYRGLJNAv@Bj@CFQIa@XCj@MV@H^x@F\\ZdAP`@NRHTYOOA?CIBWAc@K[OIMWOKB[EY@QGy@gAGE[K]eAGGe@K_AJSFyAjAm@FQNO\\Sp@Cp@JpAFTD@@EDWXcARUJ@ZZLJn@v@FF^b@ZHTPj@nA`@t@NRJ\\?TFb@dAbDF`@Tn@Vt@Xl@d@vAn@|@t@hBBn@Ff@DZVx@?\\ARPRZVh@x@l@nAPLHBLHTJl@J|@f@\\@DCLIb@g@Pa@ViARsBZgCVwAPm@b@}@bAyAh@e@l@[L_@Js@^_BVcARe@XmALU^oAHMN_@Tu@p@}@PMvAiDXcAL]Zc@VYHAXFDDHW@";
        IEnumerable<NetTopologySuite.Geometries.Coordinate> line = GeoSpatialFunctions.DecodePolyLine(polyline);
        NetTopologySuite.Geometries.Coordinate point = new NetTopologySuite.Geometries.Coordinate(13.09363, 63.39677);
        Assert.Contains(point, line);
        Assert.True(true);
    }

    [Fact]
    public void CalculateBoundingBoxTest(){
        NetTopologySuite.Geometries.Coordinate point = new NetTopologySuite.Geometries.Coordinate(13.09363, 63.39677);
        float radius = 50;
        NetTopologySuite.Geometries.Polygon box = GeoSpatialFunctions.CalculateBoundingBox(point, radius);
        Coordinate pointCord = new Coordinate(point.X, point.Y);
        Coordinate boxCord = new Coordinate(box.Coordinates[0].X, box.Coordinates[0].Y);
        double distance = GeoSpatialFunctions.DistanceTo(pointCord, boxCord);
        Assert.True(distance > radius);
        Assert.True(distance < 2*radius);
    }

    [Fact]
    public void FindPeaksTest(){
        string tott = @"{
                ""id"": 3178354862,
                ""elevation"": ""826"",
                ""name"": ""Totthummeln"",
                ""name_sapmi"": null,
                ""name_alt"": null,
                ""location"": {
                    ""coordinates"": [
                        13.1111956,
                        63.4001295
                    ],
                    ""type"": ""Point""
                }
            }
        ";
        Peak peak = JsonSerializer.Deserialize<Peak>(tott);
        Peak[] peaks = new Peak[] {peak};

        string polylineString = "o_}aKcq|nAAASZU`@Yt@OVu@nBSRIo@Me@IKE?GEM?MMMEEG?o@ASGMC@EIAHG@CAMo@ECIc@CYIM?}@KM@WMeA?e@Ci@JqA?oBBY?YFe@AYGMKBCAGICHEEAG?QDWBcAN}@J}BHq@@a@Ji@@MEIB]Ji@FKB?CM?QFa@DKLMDM@g@RaAHg@Bg@Pu@HgBN_AAEGBa@XU@EBICQNKCEIE[Hq@H}ABo@Ac@B{@GiA@k@E]LcA@WFs@?WFYDcBC{ACQBa@VwBt@aEL}ATsADaAJw@DyADW?QHo@Ay@BWDuAFYHw@FYBm@DU@o@BCGsBEMECOACDEAEJKDCFKJe@AMKKEMDOGM@CIYIGEg@u@KIIOSOOEMOOEe@]WWQa@UK[?KCKDMAQKOSWq@[wAISi@k@iA_AUg@QKM?a@OIMWCa@d@[n@Y\\O\\KNWh@GTOfAAn@K|@i@hAq@x@{ArAm@p@g@r@s@p@[d@E^Jj@?dAJPp@^FCNAND^CN@DOFEPADCFWHAF@B?HWP]BCHDLONDJC\\]DDJKH?PFP?n@O^BVHXXb@VR\\JENBvAsFVr@I^Nr@GpBHrCyAVHTBhAISe@n@EXBfBJp@Bj@W~BUT?DETOPEPUHGJMFMGGRYtEE~AGZ[|CE~ADv@AhAGtAIn@ILGV?LDh@Al@QbC@jAIv@Cv@Bn@Hr@I^Iv@DpAGjBBxCDn@Nx@BlBCTYp@BZCTJh@?bAJf@@LCNKRCRGJGRE@IIKHC?CJMKCB@PTr@JPGHQISBEASw@GKYRGLJNAv@Bj@CFQIa@XCj@MV@H^x@F\\ZdAP`@NRHTYOOA?CIBWAc@K[OIMWOKB[EY@QGy@gAGE[K]eAGGe@K_AJSFyAjAm@FQNO\\Sp@Cp@JpAFTD@@EDWXcARUJ@ZZLJn@v@FF^b@ZHTPj@nA`@t@NRJ\\?TFb@dAbDF`@Tn@Vt@Xl@d@vAn@|@t@hBBn@Ff@DZVx@?\\ARPRZVh@x@l@nAPLHBLHTJl@J|@f@\\@DCLIb@g@Pa@ViARsBZgCVwAPm@b@}@bAyAh@e@l@[L_@Js@^_BVcARe@XmALU^oAHMN_@Tu@p@}@PMvAiDXcAL]Zc@VYHAXFDDHW@";
        List<Peak> matches = GeoSpatialFunctions.FindPeaks(peaks, polylineString);
        Assert.Equal("Totthummeln", matches[0].name);
    }

    [Fact]
    public void DistanceToTest(){
        Coordinate point1 = new Coordinate(13.09363, 63.39677);
        Coordinate point2 = new Coordinate(13.09363, 63.39677);
        Coordinate point3 = new Coordinate(13.11212, 63.40841);
        double distance = GeoSpatialFunctions.DistanceTo(point1, point2);
        // Should be 2,4km according to https://www.calculator.net/distance-calculator.html?type=3&la1=13.09363&lo1=63.39677&la2=13.11212&lo2=63.40841&ctype=dec&lad1=38&lam1=53&las1=51.36&lau1=n&lod1=77&lom1=2&los1=11.76&lou1=w&lad2=39&lam2=56&las2=58.56&lau2=n&lod2=75&lom2=9&los2=1.08&lou2=w&x=91&y=12#latlog
        double distance2 = GeoSpatialFunctions.DistanceTo(point1, point3);
        Assert.Equal(0, distance);
        Assert.True(distance2 > 2350);
        Assert.True(distance2 < 2450);
    }
}
