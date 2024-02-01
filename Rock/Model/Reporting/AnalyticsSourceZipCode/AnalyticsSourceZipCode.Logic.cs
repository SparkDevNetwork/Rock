// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

using EntityFramework.Utilities;

using Newtonsoft.Json;

using OfficeOpenXml;

using RestSharp;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// AnalyticsSourceZipCode Logic
    /// </summary>
    public partial class AnalyticsSourceZipCode
    {
        /// <summary>
        /// Saves the analytics source zip code data.
        /// </summary>
        public static void GenerateAnalyticsSourceZipCodeData()
        {
            // remove all the rows and rebuild
            ClearTable();

            var analyticsZipCodes = GetZipCodeCensusData();

            SaveBoundaryAndCensusData( analyticsZipCodes );
        }

        /// <summary>
        /// Combines the Census and boundary components into AnalyticsSourceZipCode entries.
        /// </summary>
        /// <param name="analyticsZipCodes">The analytics zip codes.</param>
        /// <param name="zipCodeBoundaries">The zip code boundaries.</param>
        public static void SaveBoundaryAndCensusData( List<AnalyticsSourceZipCode> analyticsZipCodes, List<ZipCodeBoundary> zipCodeBoundaries = null )
        {
            // Update census data with ZipCode details
            if ( zipCodeBoundaries?.Any() == true )
            {
                foreach ( var analyticsZipCode in analyticsZipCodes )
                {
                    var zipCodeBoundary = zipCodeBoundaries.Find( z => z.ZipCode == analyticsZipCode.ZipCode );

                    if ( zipCodeBoundary != null )
                    {
                        analyticsZipCode.State = zipCodeBoundary.State ?? string.Empty;
                        analyticsZipCode.SquareMiles = zipCodeBoundary.SquareMiles;
                        analyticsZipCode.City = zipCodeBoundary.City ?? string.Empty;
                        analyticsZipCode.GeoFence = zipCodeBoundary.GeoFence;
                    }
                }

                // NOTE: We can't use rockContext.BulkInsert because that enforces that the <T> is Rock.Data.IEntity, and using the EFBatchOperation
                // results in a NullPointer when saving DbGeography because the GetValue method on the EFDataReader tries to access the Latitude and
                // Longitude data on the DbGeography, which doesn't exist for polygon based DbGeography.

                // Batch size
                int batchSize = 1000;
                RockContext rockContext = new RockContext();

                // Calculate the number of batches
                int batches = ( int ) Math.Ceiling( ( double ) analyticsZipCodes.Count / batchSize );
                for ( int i = 0; i < batches; i++ )
                {
                    rockContext = new RockContext();
                    rockContext.Configuration.AutoDetectChangesEnabled = false;
                    rockContext.Configuration.ValidateOnSaveEnabled = false;

                    // Get the current batch
                    var currentBatch = analyticsZipCodes.Skip( i * batchSize ).Take( batchSize ).ToList();

                    rockContext.AnalyticsSourceZipCodes.AddRange( currentBatch );
                    rockContext.SaveChanges();
                }

                rockContext.Dispose();
            }
            else
            {
                using ( var rockContext = new RockContext() )
                {
                    // Since we are not saving the DbGeography details we'll just use EFBatchOperation to BulkInsert.
                    EFBatchOperation.For( rockContext, rockContext.AnalyticsSourceZipCodes ).InsertAll( analyticsZipCodes );
                }
            }
        }

        /// <summary>
        /// Clears the table of all data so it can be repopulated.
        /// </summary>
        public static void ClearTable()
        {
            using ( var rockContext = new RockContext() )
            {
                try
                {
                    // if TRUNCATE takes more than 5 seconds, it is probably due to a lock. If so, do a DELETE FROM instead
                    rockContext.Database.CommandTimeout = 5;
                    rockContext.Database.ExecuteSqlCommand( string.Format( "TRUNCATE TABLE {0}", typeof( AnalyticsSourceZipCode ).GetCustomAttribute<TableAttribute>().Name ) );
                }
                catch
                {
                    rockContext.Database.CommandTimeout = null;
                    rockContext.Database.ExecuteSqlCommand( string.Format( "DELETE FROM {0}", typeof( AnalyticsSourceZipCode ).GetCustomAttribute<TableAttribute>().Name ) );
                }
            }
        }

        /// <summary>
        /// Reads the Census data component from disk.
        /// </summary>
        /// <returns></returns>
        public static List<AnalyticsSourceZipCode> GetZipCodeCensusData()
        {
            var path = System.IO.Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "Content\\InternalSite\\Formatted_Census_Data.xlsx" );
            var fileInfo = new FileInfo( path );

            using ( var excelPackage = new ExcelPackage( fileInfo ) )
            {
                var table = new DataTable();
                var workSheet = excelPackage.Workbook.Worksheets[1];

                //check if the worksheet is completely empty
                if ( workSheet.Dimension == null )
                {
                    return new List<AnalyticsSourceZipCode>();
                }

                int noOfCol = workSheet.Dimension.End.Column;
                int noOfRow = workSheet.Dimension.End.Row;
                int rowIndex = 1;

                // Get headers
                for ( int c = 1; c <= noOfCol; c++ )
                {
                    table.Columns.Add( workSheet.Cells[rowIndex, c].Text );
                }

                // Get rows
                rowIndex = 2;
                for ( int r = rowIndex; r <= noOfRow; r++ )
                {
                    var dr = table.NewRow();
                    for ( int c = 1; c <= noOfCol; c++ )
                    {
                        dr[c - 1] = workSheet.Cells[r, c].Value;
                    }
                    table.Rows.Add( dr );
                }

                // Deserialize data as AnalyticsSourceZipCode and ignore any parsing errors fro DataType mismatch, some decimal columns contain string '**'
                // values so we ignore these and leave their values as default.
                var data = JsonConvert.DeserializeObject<List<AnalyticsSourceZipCode>>( JsonConvert.SerializeObject( table ), new JsonSerializerSettings()
                {
                    Error = HandleDeserializationError
                } );

                return data.OrderBy( z => z.ZipCode ).ToList();
            }
        }

#pragma warning disable CS1587
        /**
        * 01/08/2024 - KA
        * 
        * The download method is commented out at the moment due to the large file that would otherwise be downloaded (over 900MB).
        * There might be a future change to allow end users to selectively choose to download the boundary data.
        */

        /// <summary>
        /// Downloads the Boundary data as a memory stream.
        /// </summary>
        /// <returns><see cref="MemoryStream"/></returns>
        /// <exception cref="System.Net.Http.HttpRequestException">Unable to download Zip Code boundary data.</exception>
        //public static List<ZipCodeBoundary> DownloadZipCodeBoundaryData()
        //{
        //    // Set client to timeout after 5 minutes.
        //    var client = new RestClient( "https://opendata.arcgis.com/api/v3/datasets" )
        //    {
        //        Timeout = 300000
        //    };

        //    // Send request to ArcGIS Hub (https://hub.arcgis.com/datasets/d6f7ee6129e241cc9b6f75978e47128b_0/about) to download
        //    // ZipCode boundary dataset. The endpoint below is the same endpoint used when downloading ZipCode boundary data from their
        //    // web app.
        //    var request = new RestRequest( "/d6f7ee6129e241cc9b6f75978e47128b_0/downloads/data?format=kml&spatialRefId=4326&where=1%3D1" );
        //    var data = new List<ZipCodeBoundary>();

        //    var response = client.DownloadData( request );

        //    if ( response != null )
        //    {
        //        var serializer = new XmlSerializer( typeof( Kml ) );

        //        using ( var stream = new MemoryStream() )
        //        {
        //            stream.Write( response, 0, response.Length );
        //            stream.Position = 0;

        //            var kml = ( Kml ) serializer.Deserialize( stream );

        //            foreach ( var placemark in kml.Document.Folder.Placemarks )
        //            {
        //                var simpleDataList = placemark.ExtendedData.SchemaData.SimpleDataList;
        //                var boundaryData = new ZipCodeBoundary()
        //                {
        //                    ZipCode = simpleDataList.Find( x => x.Name == "ZIP_CODE" )?.Value,
        //                    City = simpleDataList.Find( x => x.Name == "PO_NAME" )?.Value,
        //                    State = simpleDataList.Find( x => x.Name == "STATE" )?.Value,
        //                };

        //                if ( decimal.TryParse( simpleDataList.Find( x => x.Name == "SQMI" ).Value, out decimal squareMiles ) )
        //                {
        //                    boundaryData.SquareMiles = squareMiles;
        //                }

        //                var polygon = placemark.Polygon ?? placemark.MultiGeometry?.Polygon;
        //                if ( polygon != null )
        //                {
        //                    boundaryData.GeoFence = CalculateGeoFence( polygon.OuterBoundaryIs?.LinearRing?.Coordinates );
        //                }

        //                data.Add( boundaryData );
        //            }
        //        }
        //    }

        //    return data.OrderBy( z => z.ZipCode ).ToList();
        //}

#pragma warning restore CS1587

        /// <summary>
        /// Set serialization errors as handled so properties that cannot be parsed are left as their default value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="errorArgs"></param>
        private static void HandleDeserializationError( object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs )
        {
            errorArgs.ErrorContext.Handled = true;
        }

        /// <summary>
        /// Calculates the geo fence from the given coordinates.
        /// </summary>
        /// <param name="coordinates">The coordinates.</param>
        /// <returns><see cref="DbGeography"/></returns>
        private static DbGeography CalculateGeoFence( string coordinates )
        {
            if ( !string.IsNullOrWhiteSpace( coordinates ) )
            {
                // Assuming the coordinates are in the format "longitude1,latitude1,0 longitude2,latitude2,0 ..."
                string[] pointStrings = coordinates.Split( ' ' );

                if ( pointStrings.Length >= 2 )
                {
                    // Extract latitude and longitude pairs
                    var points = pointStrings.Select( pointString =>
                    {
                        string[] values = pointString.Split( ',' );
                        double longitude = double.Parse( values[0] );
                        double latitude = double.Parse( values[1] );
                        return new Tuple<double, double>( latitude, longitude );
                    } ).ToList();

                    // Create the WKT representation of the polygon
                    string wkt = $"POLYGON(({string.Join( ", ", points.Select( p => $"{p.Item2} {p.Item1}" ) )}))";

                    // Create the DbGeography instance from the WKT representation
                    return DbGeography.PolygonFromText( wkt, 4326 );
                }
            }

            return null;
        }

        /// <summary>
        /// Contains the ZipCode geographical data.
        /// </summary>
        public sealed class ZipCodeBoundary
        {
            /// <summary>
            /// Gets or sets the zip code.
            /// </summary>
            /// <value>
            /// The zip code.
            /// </value>
            [JsonProperty( "ZIP_CODE" )]
            public string ZipCode { get; set; }

            /// <summary>
            /// Gets or sets the city.
            /// </summary>
            /// <value>
            /// The city.
            /// </value>
            [JsonProperty( "PO_NAME" )]
            public string City { get; set; }

            /// <summary>
            /// Gets or sets the state.
            /// </summary>
            /// <value>
            /// The state.
            /// </value>
            [JsonProperty( "STATE" )]
            public string State { get; set; }

            /// <summary>
            /// Gets or sets the square miles.
            /// </summary>
            /// <value>
            /// The square miles.
            /// </value>
            [JsonProperty( "SQMI" )]
            public decimal SquareMiles { get; set; }

            /// <summary>
            /// Gets or sets the geo fence.
            /// </summary>
            /// <value>
            /// The geo fence.
            /// </value>
            public DbGeography GeoFence { get; set; }
        }
    }
}
