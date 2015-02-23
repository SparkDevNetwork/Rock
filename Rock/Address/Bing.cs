// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Web;
using System.Web;

using Rock;
using Rock.Attribute;
using Rock.Web.UI;

namespace Rock.Address
{
    /// <summary>
    /// The standardization/geocoding service from <a href="http://dev.virtualearth.net">Bing</a>
    /// </summary>
    [Description( "Address Standardization and Geocoding service from Bing" )]
    [Export( typeof( VerificationComponent ) )]
    [ExportMetadata( "ComponentName", "Bing" )]
    [TextField( "Bing Maps Key", "The Bing maps key", true, "", "", 1 )]
    [IntegerField( "Daily Transaction Limit", "The maximum number of transactions to process each day.", false, 5000, "", 2 )]
    public class Bing : VerificationComponent
    {
        const string TXN_DATE = "com.rockrms.bing.txnDate";
        const string DAILY_TXN_COUNT = "com.rockrms.bing.dailyTxnCount";

        /// <summary>
        /// Standardizes and Geocodes an address using Bing service
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="reVerify">Should location be reverified even if it has already been succesfully verified</param>
        /// <param name="result">The result code unique to the service.</param>
        /// <returns>
        /// True/False value of whether the verification was successfull or not
        /// </returns>
        public override bool VerifyLocation( Rock.Model.Location location, bool reVerify, out string result )
        {
            bool verified = false;
            result = string.Empty;

            // Only verify if location is valid, has not been locked, and 
            // has either never been attempted or last attempt was in last 30 secs (prev active service failed) or reverifying
            if ( location != null &&
                !( location.IsGeoPointLocked ?? false ) &&
                (
                    !location.GeocodeAttemptedDateTime.HasValue ||
                    location.GeocodeAttemptedDateTime.Value.CompareTo( RockDateTime.Now.AddSeconds( -30 ) ) > 0 ||
                    reVerify
                ) )
            {
                // Verify that bing transaction count hasn't been exceeded for the day
                DateTime? txnDate = Rock.Web.SystemSettings.GetValue( TXN_DATE ).AsDateTime();
                int? dailyTxnCount = 0;

                if ( txnDate.Equals( RockDateTime.Today ) )
                {
                    dailyTxnCount = Rock.Web.SystemSettings.GetValue( DAILY_TXN_COUNT ).AsIntegerOrNull();
                }
                else
                {
                    Rock.Web.SystemSettings.SetValue( TXN_DATE, RockDateTime.Today.ToShortDateString() );
                }

                int? maxTxnCount = GetAttributeValue( "DailyTransactionLimit" ).AsIntegerOrNull();

                if ( !maxTxnCount.HasValue || maxTxnCount.Value == 0 || dailyTxnCount < maxTxnCount.Value )
                {
                    dailyTxnCount++;
                    Rock.Web.SystemSettings.SetValue( DAILY_TXN_COUNT, dailyTxnCount.ToString() );

                    string key = GetAttributeValue( "BingMapsKey" );

                    var queryValues = new Dictionary<string, string>();
                    queryValues.Add( "adminDistrict", location.State );
                    queryValues.Add( "locality", location.City );
                    queryValues.Add( "postalCode", location.PostalCode );
                    queryValues.Add( "addressLine", location.Street1 + " " + location.Street2 );
                    queryValues.Add( "countryRegion", location.Country );

                    var queryParams = new List<string>();
                    foreach ( var queryKeyValue in queryValues )
                    {
                        if ( !string.IsNullOrWhiteSpace( queryKeyValue.Value ) )
                        {
                            queryParams.Add( string.Format( "{0}={1}", queryKeyValue.Key, HttpUtility.UrlEncode( queryKeyValue.Value.Trim() ) ) );
                        }
                    }
                    Uri geocodeRequest = new Uri( string.Format( "http://dev.virtualearth.net/REST/v1/Locations?{0}&key={1}", queryParams.AsDelimited( "&" ), key ) );

                    WebClient wc = new WebClient();
                    var stream = wc.OpenRead( geocodeRequest );

                    DataContractJsonSerializer ser = new DataContractJsonSerializer( typeof( Response ) );
                    var x = ser.ReadObject( stream ) as Response;
                    if ( x != null )
                    {
                        if ( x.ResourceSets.Length > 0 &&
                            x.ResourceSets[0].Resources.Length == 1 )
                        {
                            var bingLocation = (Location)x.ResourceSets[0].Resources[0];
                            var matchCodes = bingLocation.MatchCodes.ToList();

                            result = string.Format( "Confidence: {0}; MatchCodes: {1}",
                                bingLocation.Confidence, matchCodes.AsDelimited( "," ) );

                            if ( bingLocation.Confidence == "High" && matchCodes.Contains( "Good" ) )
                            {
                                location.SetLocationPointFromLatLong( bingLocation.Point.Coordinates[0], bingLocation.Point.Coordinates[1] );
                                location.GeocodedDateTime = RockDateTime.Now;
                                verified = true;

                                if ( !location.StandardizedDateTime.HasValue || reVerify )
                                {
                                    var address = bingLocation.Address;
                                    if ( address != null )
                                    {
                                        location.Street1 = address.AddressLine;
                                        location.City = address.Locality;
                                        location.State = address.AdminDistrict;
                                        if ( !String.IsNullOrWhiteSpace( address.PostalCode ) &&
                                            !( ( location.PostalCode ?? string.Empty ).StartsWith( address.PostalCode ) ) )
                                        {
                                            location.PostalCode = address.PostalCode;
                                        }
                                        location.StandardizeAttemptedServiceType = "Bing";
                                        location.StandardizeAttemptedResult = "High";
                                        location.StandardizedDateTime = RockDateTime.Now;
                                    }
                                }

                            }
                        }
                        else
                        {
                            result = "Zero or More than 1 result";
                        }
                    }
                    else
                    {
                        result = "Invalid response";
                    }
                }
                else
                {
                    result = "Daily transaction limit exceeded";
                }

                location.GeocodeAttemptedServiceType = "Bing";
                location.GeocodeAttemptedDateTime = RockDateTime.Now;
                location.GeocodeAttemptedResult = result;
            }

            return verified;
        }

        private void GetResponse( Uri uri, Action<Response> callback )
        {
            WebClient wc = new WebClient();
            wc.OpenReadCompleted += ( o, a ) =>
            {
                if ( callback != null )
                {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer( typeof( Response ) );
                    callback( ser.ReadObject( a.Result ) as Response );
                }
            };
            wc.OpenRead( uri );
        }
    }

#pragma warning disable

    [DataContract]
    public class Address
    {
        [DataMember( Name = "addressLine", EmitDefaultValue = false )]
        public string AddressLine { get; set; }

        [DataMember( Name = "adminDistrict", EmitDefaultValue = false )]
        public string AdminDistrict { get; set; }

        [DataMember( Name = "adminDistrict2", EmitDefaultValue = false )]
        public string AdminDistrict2 { get; set; }

        [DataMember( Name = "countryRegion", EmitDefaultValue = false )]
        public string CountryRegion { get; set; }

        [DataMember( Name = "formattedAddress", EmitDefaultValue = false )]
        public string FormattedAddress { get; set; }

        [DataMember( Name = "locality", EmitDefaultValue = false )]
        public string Locality { get; set; }

        [DataMember( Name = "postalCode", EmitDefaultValue = false )]
        public string PostalCode { get; set; }

        [DataMember( Name = "neighborhood", EmitDefaultValue = false )]
        public string Neighborhood { get; set; }

        [DataMember( Name = "landmark", EmitDefaultValue = false )]
        public string Landmark { get; set; }
    }

    [DataContract( Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1" )]
    public class BirdseyeMetadata : ImageryMetadata
    {
        [DataMember( Name = "orientation", EmitDefaultValue = false )]
        public double Orientation { get; set; }

        [DataMember( Name = "tilesX", EmitDefaultValue = false )]
        public int TilesX { get; set; }

        [DataMember( Name = "tilesY", EmitDefaultValue = false )]
        public int TilesY { get; set; }
    }

    [DataContract]
    public class BoundingBox
    {
        [DataMember( Name = "southLatitude", EmitDefaultValue = false )]
        public double SouthLatitude { get; set; }

        [DataMember( Name = "westLongitude", EmitDefaultValue = false )]
        public double WestLongitude { get; set; }

        [DataMember( Name = "northLatitude", EmitDefaultValue = false )]
        public double NorthLatitude { get; set; }

        [DataMember( Name = "eastLongitude", EmitDefaultValue = false )]
        public double EastLongitude { get; set; }
    }

    [DataContract]
    public class Detail
    {
        [DataMember( Name = "compassDegrees", EmitDefaultValue = false )]
        public int CompassDegrees { get; set; }

        [DataMember( Name = "maneuverType", EmitDefaultValue = false )]
        public string ManeuverType { get; set; }

        [DataMember( Name = "startPathIndices", EmitDefaultValue = false )]
        public int[] StartPathIndices { get; set; }

        [DataMember( Name = "endPathIndices", EmitDefaultValue = false )]
        public int[] EndPathIndices { get; set; }

        [DataMember( Name = "roadType", EmitDefaultValue = false )]
        public string RoadType { get; set; }

        [DataMember( Name = "locationCodes", EmitDefaultValue = false )]
        public string[] LocationCodes { get; set; }

        [DataMember( Name = "names", EmitDefaultValue = false )]
        public string[] Names { get; set; }

        [DataMember( Name = "mode", EmitDefaultValue = false )]
        public string Mode { get; set; }

        [DataMember( Name = "roadShieldRequestParameters", EmitDefaultValue = false )]
        public RoadShield roadShieldRequestParameters { get; set; }
    }

    [DataContract]
    public class Generalization
    {
        [DataMember( Name = "pathIndices", EmitDefaultValue = false )]
        public int[] PathIndices { get; set; }

        [DataMember( Name = "latLongTolerance", EmitDefaultValue = false )]
        public double LatLongTolerance { get; set; }
    }

    [DataContract]
    public class Hint
    {
        [DataMember( Name = "hintType", EmitDefaultValue = false )]
        public string HintType { get; set; }

        [DataMember( Name = "text", EmitDefaultValue = false )]
        public string Text { get; set; }
    }

    [DataContract( Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1" )]
    [KnownType( typeof( StaticMapMetadata ) )]
    [KnownType( typeof( BirdseyeMetadata ) )]
    public class ImageryMetadata : Resource
    {
        [DataMember( Name = "imageHeight", EmitDefaultValue = false )]
        public string ImageHeight { get; set; }

        [DataMember( Name = "imageWidth", EmitDefaultValue = false )]
        public string ImageWidth { get; set; }

        [DataMember( Name = "imageUrl", EmitDefaultValue = false )]
        public string ImageUrl { get; set; }

        [DataMember( Name = "imageUrlSubdomains", EmitDefaultValue = false )]
        public string[] ImageUrlSubdomains { get; set; }

        [DataMember( Name = "vintageEnd", EmitDefaultValue = false )]
        public string VintageEnd { get; set; }

        [DataMember( Name = "vintageStart", EmitDefaultValue = false )]
        public string VintageStart { get; set; }

        [DataMember( Name = "zoomMax", EmitDefaultValue = false )]
        public int ZoomMax { get; set; }

        [DataMember( Name = "zoomMin", EmitDefaultValue = false )]
        public int ZoomMin { get; set; }
    }

    [DataContract]
    public class Instruction
    {
        [DataMember( Name = "maneuverType", EmitDefaultValue = false )]
        public string ManeuverType { get; set; }

        [DataMember( Name = "text", EmitDefaultValue = false )]
        public string Text { get; set; }
    }

    [DataContract]
    public class ItineraryItem
    {
        [DataMember( Name = "childItineraryItems", EmitDefaultValue = false )]
        public ItineraryItem ChildItineraryItems { get; set; }

        [DataMember( Name = "compassDirection", EmitDefaultValue = false )]
        public string CompassDirection { get; set; }

        [DataMember( Name = "details", EmitDefaultValue = false )]
        public Detail[] Details { get; set; }

        [DataMember( Name = "exit", EmitDefaultValue = false )]
        public string Exit { get; set; }

        [DataMember( Name = "hints", EmitDefaultValue = false )]
        public Hint[] Hints { get; set; }

        [DataMember( Name = "iconType", EmitDefaultValue = false )]
        public string IconType { get; set; }

        [DataMember( Name = "instruction", EmitDefaultValue = false )]
        public Instruction Instruction { get; set; }

        [DataMember( Name = "maneuverPoint", EmitDefaultValue = false )]
        public Point ManeuverPoint { get; set; }

        [DataMember( Name = "sideOfStreet", EmitDefaultValue = false )]
        public string SideOfStreet { get; set; }

        [DataMember( Name = "signs", EmitDefaultValue = false )]
        public string[] Signs { get; set; }

        [DataMember( Name = "time", EmitDefaultValue = false )]
        public string Time { get; set; }

        [DataMember( Name = "tollZone", EmitDefaultValue = false )]
        public string TollZone { get; set; }

        [DataMember( Name = "towardsRoadName", EmitDefaultValue = false )]
        public string TowardsRoadName { get; set; }

        [DataMember( Name = "transitLine", EmitDefaultValue = false )]
        public TransitLine TransitLine { get; set; }

        [DataMember( Name = "transitStopId", EmitDefaultValue = false )]
        public int TransitStopId { get; set; }

        [DataMember( Name = "transitTerminus", EmitDefaultValue = false )]
        public string TransitTerminus { get; set; }

        [DataMember( Name = "travelDistance", EmitDefaultValue = false )]
        public double TravelDistance { get; set; }

        [DataMember( Name = "travelDuration", EmitDefaultValue = false )]
        public double TravelDuration { get; set; }

        [DataMember( Name = "travelMode", EmitDefaultValue = false )]
        public string TravelMode { get; set; }

        [DataMember( Name = "warning", EmitDefaultValue = false )]
        public Warning[] Warning { get; set; }
    }

    [DataContract]
    public class Line
    {
        [DataMember( Name = "type", EmitDefaultValue = false )]
        public string Type { get; set; }

        [DataMember( Name = "coordinates", EmitDefaultValue = false )]
        public double[][] Coordinates { get; set; }
    }

    [DataContract( Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1" )]
    public class Location : Resource
    {
        [DataMember( Name = "name", EmitDefaultValue = false )]
        public string Name { get; set; }

        [DataMember( Name = "point", EmitDefaultValue = false )]
        public Point Point { get; set; }

        [DataMember( Name = "entityType", EmitDefaultValue = false )]
        public string EntityType { get; set; }

        [DataMember( Name = "address", EmitDefaultValue = false )]
        public Address Address { get; set; }

        [DataMember( Name = "confidence", EmitDefaultValue = false )]
        public string Confidence { get; set; }

        [DataMember( Name = "matchCodes", EmitDefaultValue = false )]
        public string[] MatchCodes { get; set; }

        [DataMember( Name = "geocodePoints", EmitDefaultValue = false )]
        public Point[] GeocodePoints { get; set; }
    }

    [DataContract( Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1" )]
    public class PinInfo
    {
        [DataMember( Name = "anchor", EmitDefaultValue = false )]
        public Pixel Anchor { get; set; }

        [DataMember( Name = "bottomRightOffset", EmitDefaultValue = false )]
        public Pixel BottomRightOffset { get; set; }

        [DataMember( Name = "topLeftOffset", EmitDefaultValue = false )]
        public Pixel TopLeftOffset { get; set; }

        [DataMember( Name = "point", EmitDefaultValue = false )]
        public Point Point { get; set; }
    }

    [DataContract( Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1" )]
    public class Pixel
    {
        [DataMember( Name = "x", EmitDefaultValue = false )]
        public string X { get; set; }

        [DataMember( Name = "y", EmitDefaultValue = false )]
        public string Y { get; set; }
    }

    [DataContract]
    public class Point : Shape
    {
        [DataMember( Name = "type", EmitDefaultValue = false )]
        public string Type { get; set; }

        /// <summary>
        /// Latitude,Longitude
        /// </summary>
        [DataMember( Name = "coordinates", EmitDefaultValue = false )]
        public double[] Coordinates { get; set; }

        [DataMember( Name = "calculationMethod", EmitDefaultValue = false )]
        public string CalculationMethod { get; set; }

        [DataMember( Name = "usageTypes", EmitDefaultValue = false )]
        public string[] UsageTypes { get; set; }
    }

    [DataContract]
    [KnownType( typeof( Location ) )]
    [KnownType( typeof( Route ) )]
    [KnownType( typeof( TrafficIncident ) )]
    [KnownType( typeof( ImageryMetadata ) )]
    [KnownType( typeof( ElevationData ) )]
    [KnownType( typeof( SeaLevelData ) )]
    [KnownType( typeof( CompressedPointList ) )]
    public class Resource
    {
        [DataMember( Name = "bbox", EmitDefaultValue = false )]
        public double[] BoundingBox { get; set; }

        [DataMember( Name = "__type", EmitDefaultValue = false )]
        public string Type { get; set; }
    }

    [DataContract]
    public class ResourceSet
    {
        [DataMember( Name = "estimatedTotal", EmitDefaultValue = false )]
        public long EstimatedTotal { get; set; }

        [DataMember( Name = "resources", EmitDefaultValue = false )]
        public Resource[] Resources { get; set; }
    }

    [DataContract]
    public class Response
    {
        [DataMember( Name = "copyright", EmitDefaultValue = false )]
        public string Copyright { get; set; }

        [DataMember( Name = "brandLogoUri", EmitDefaultValue = false )]
        public string BrandLogoUri { get; set; }

        [DataMember( Name = "statusCode", EmitDefaultValue = false )]
        public int StatusCode { get; set; }

        [DataMember( Name = "statusDescription", EmitDefaultValue = false )]
        public string StatusDescription { get; set; }

        [DataMember( Name = "authenticationResultCode", EmitDefaultValue = false )]
        public string AuthenticationResultCode { get; set; }

        [DataMember( Name = "errorDetails", EmitDefaultValue = false )]
        public string[] errorDetails { get; set; }

        [DataMember( Name = "traceId", EmitDefaultValue = false )]
        public string TraceId { get; set; }

        [DataMember( Name = "resourceSets", EmitDefaultValue = false )]
        public ResourceSet[] ResourceSets { get; set; }
    }

    [DataContract]
    public class RoadShield
    {
        [DataMember( Name = "bucket", EmitDefaultValue = false )]
        public int Bucket { get; set; }

        [DataMember( Name = "shields", EmitDefaultValue = false )]
        public Shield[] Shields { get; set; }
    }

    [DataContract( Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1" )]
    public class Route : Resource
    {
        [DataMember( Name = "id", EmitDefaultValue = false )]
        public string Id { get; set; }

        [DataMember( Name = "distanceUnit", EmitDefaultValue = false )]
        public string DistanceUnit { get; set; }

        [DataMember( Name = "durationUnit", EmitDefaultValue = false )]
        public string DurationUnit { get; set; }

        [DataMember( Name = "travelDistance", EmitDefaultValue = false )]
        public double TravelDistance { get; set; }

        [DataMember( Name = "travelDuration", EmitDefaultValue = false )]
        public double TravelDuration { get; set; }

        [DataMember( Name = "routeLegs", EmitDefaultValue = false )]
        public RouteLeg[] RouteLegs { get; set; }

        [DataMember( Name = "routePath", EmitDefaultValue = false )]
        public RoutePath RoutePath { get; set; }
    }

    [DataContract]
    public class RouteLeg
    {
        [DataMember( Name = "travelDistance", EmitDefaultValue = false )]
        public double TravelDistance { get; set; }

        [DataMember( Name = "travelDuration", EmitDefaultValue = false )]
        public double TravelDuration { get; set; }

        [DataMember( Name = "actualStart", EmitDefaultValue = false )]
        public Point ActualStart { get; set; }

        [DataMember( Name = "actualEnd", EmitDefaultValue = false )]
        public Point ActualEnd { get; set; }

        [DataMember( Name = "startLocation", EmitDefaultValue = false )]
        public Location StartLocation { get; set; }

        [DataMember( Name = "endLocation", EmitDefaultValue = false )]
        public Location EndLocation { get; set; }

        [DataMember( Name = "itineraryItems", EmitDefaultValue = false )]
        public ItineraryItem[] ItineraryItems { get; set; }
    }

    [DataContract]
    public class RoutePath
    {
        [DataMember( Name = "line", EmitDefaultValue = false )]
        public Line Line { get; set; }

        [DataMember( Name = "generalizations", EmitDefaultValue = false )]
        public Generalization[] Generalizations { get; set; }
    }

    [DataContract]
    [KnownType( typeof( Point ) )]
    public class Shape
    {
        [DataMember( Name = "boundingBox", EmitDefaultValue = false )]
        public double[] BoundingBox { get; set; }
    }

    [DataContract]
    public class Shield
    {
        [DataMember( Name = "labels", EmitDefaultValue = false )]
        public string[] Labels { get; set; }

        [DataMember( Name = "roadShieldType", EmitDefaultValue = false )]
        public int RoadShieldType { get; set; }
    }

    [DataContract( Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1" )]
    public class StaticMapMetadata : ImageryMetadata
    {
        [DataMember( Name = "mapCenter", EmitDefaultValue = false )]
        public Point MapCenter { get; set; }

        [DataMember( Name = "pushpins", EmitDefaultValue = false )]
        public PinInfo[] Pushpins { get; set; }

        [DataMember( Name = "zoom", EmitDefaultValue = false )]
        public string Zoom { get; set; }
    }

    [DataContract( Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1" )]
    public class TrafficIncident : Resource
    {
        [DataMember( Name = "point", EmitDefaultValue = false )]
        public Point Point { get; set; }

        [DataMember( Name = "congestion", EmitDefaultValue = false )]
        public string Congestion { get; set; }

        [DataMember( Name = "description", EmitDefaultValue = false )]
        public string Description { get; set; }

        [DataMember( Name = "detour", EmitDefaultValue = false )]
        public string Detour { get; set; }

        [DataMember( Name = "start", EmitDefaultValue = false )]
        public string Start { get; set; }

        [DataMember( Name = "end", EmitDefaultValue = false )]
        public string End { get; set; }

        [DataMember( Name = "incidentId", EmitDefaultValue = false )]
        public long IncidentId { get; set; }

        [DataMember( Name = "lane", EmitDefaultValue = false )]
        public string Lane { get; set; }

        [DataMember( Name = "lastModified", EmitDefaultValue = false )]
        public string LastModified { get; set; }

        [DataMember( Name = "roadClosed", EmitDefaultValue = false )]
        public bool RoadClosed { get; set; }

        [DataMember( Name = "severity", EmitDefaultValue = false )]
        public int Severity { get; set; }

        [DataMember( Name = "toPoint", EmitDefaultValue = false )]
        public Point ToPoint { get; set; }

        [DataMember( Name = "locationCodes", EmitDefaultValue = false )]
        public string[] LocationCodes { get; set; }

        [DataMember( Name = "type", EmitDefaultValue = false )]
        public int Type { get; set; }

        [DataMember( Name = "verified", EmitDefaultValue = false )]
        public bool Verified { get; set; }
    }

    [DataContract]
    public class TransitLine
    {
        [DataMember( Name = "verboseName", EmitDefaultValue = false )]
        public string verboseName { get; set; }

        [DataMember( Name = "abbreviatedName", EmitDefaultValue = false )]
        public string abbreviatedName { get; set; }

        [DataMember( Name = "agencyId", EmitDefaultValue = false )]
        public long AgencyId { get; set; }

        [DataMember( Name = "agencyName", EmitDefaultValue = false )]
        public string agencyName { get; set; }

        [DataMember( Name = "lineColor", EmitDefaultValue = false )]
        public long lineColor { get; set; }

        [DataMember( Name = "lineTextColor", EmitDefaultValue = false )]
        public long lineTextColor { get; set; }

        [DataMember( Name = "uri", EmitDefaultValue = false )]
        public string uri { get; set; }

        [DataMember( Name = "phoneNumber", EmitDefaultValue = false )]
        public string phoneNumber { get; set; }

        [DataMember( Name = "providerInfo", EmitDefaultValue = false )]
        public string providerInfo { get; set; }
    }

    [DataContract]
    public class Warning
    {
        [DataMember( Name = "warningType", EmitDefaultValue = false )]
        public string WarningType { get; set; }

        [DataMember( Name = "severity", EmitDefaultValue = false )]
        public string Severity { get; set; }

        [DataMember( Name = "text", EmitDefaultValue = false )]
        public string Text { get; set; }
    }

    [DataContract( Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1" )]
    public class CompressedPointList : Resource
    {
        [DataMember( Name = "value", EmitDefaultValue = false )]
        public string Value { get; set; }
    }

    [DataContract( Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1" )]
    public class ElevationData : Resource
    {
        [DataMember( Name = "elevations", EmitDefaultValue = false )]
        public int[] Elevations { get; set; }

        [DataMember( Name = "zoomLevel", EmitDefaultValue = false )]
        public int ZoomLevel { get; set; }
    }

    [DataContract( Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1" )]
    public class SeaLevelData : Resource
    {
        [DataMember( Name = "offsets", EmitDefaultValue = false )]
        public int[] Offsets { get; set; }

        [DataMember( Name = "zoomLevel", EmitDefaultValue = false )]
        public int ZoomLevel { get; set; }
    }

#pragma warning restore

}