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
        double distance = GeoSpatialFunctions.DistanceTo(point, box.Boundary.Coordinates[0]);
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
}
