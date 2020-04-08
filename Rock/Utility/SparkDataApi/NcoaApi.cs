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
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RestSharp;

using Rock.Jobs;

namespace Rock.Utility.SparkDataApi
{
    /// <summary>
    /// NCOA API calls
    /// </summary>
    public class NcoaApi
    {
        private string NCOA_SERVER = "https://app.truencoa.com";
        //private string NCOA_SERVER = "https://app.testing.truencoa.com";
        private int _batchsize = 150;
        private string _username;
        private string _password;
        private RestClient _client = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="NcoaApi"/> class.
        /// </summary>
        /// <param name="usernamePassword">NCOA's credentials.</param>
        public NcoaApi( UsernamePassword usernamePassword )
        {
            _username = usernamePassword.UserName;
            _password = usernamePassword.Password;
            CreateRestClient();
        }

        /// <summary>
        /// Creates the rest client.
        /// </summary>
        private void CreateRestClient()
        {
            _client = new RestClient( NCOA_SERVER );
            _client.AddDefaultHeader( "user_name", _username );
            _client.AddDefaultHeader( "password", _password );
            _client.AddDefaultHeader( "Content-Type", "application/x-www-form-urlencoded" );
        }

        /// <summary>
        /// Creates the NCOA file on the NCOA server.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="id">The identifier.</param>
        public void CreateFile( string fileName, string companyName, out string id )
        {
            id = null;
            try
            {
                // submit for exporting
                var request = new RestRequest( $"api/files/{fileName}/index", Method.POST );
                request.AddParameter( "application/x-www-form-urlencoded", $"caption={Uri.EscapeDataString(companyName)}", ParameterType.RequestBody );
                IRestResponse response = _client.Execute( request );
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
                    {
                        Content = new StringContent( response.Content )
                    } );
                }

                try
                {
                    NcoaResponse file = JsonConvert.DeserializeObject<NcoaResponse>( response.Content );
                    id = file.Id;
                }
                catch
                {
                    throw new Exception( $"Failed to deserialize NCOA response: {response.Content}" );
                }
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Communication with NCOA server failed: Could not create address file on the NCOA server that is required to begin processing. Possible cause is the NCOA API server is down.", ex );
            }
        }

        /// <summary>
        /// Uploads the addresses.
        /// </summary>
        /// <param name="addresses">The addresses.</param>
        /// <param name="id">Name of the file.</param>
        public void UploadAddresses( Dictionary<int, PersonAddressItem> addresses, string id )
        {
            try
            {
                string directory = AppDomain.CurrentDomain.BaseDirectory;
                directory = Path.Combine( directory, "App_Data", "Logs" );

                if ( !Directory.Exists( directory ) )
                {
                    Directory.CreateDirectory( directory );
                }

                string filePath = Path.Combine( directory, "NcoaException.log" );
                File.WriteAllText( filePath, string.Empty );

                PersonAddressItem[] addressArray = addresses.Values.ToArray();
                StringBuilder data = new StringBuilder();
                for ( int i = 1; i <= addressArray.Length; i++ )
                {
                    PersonAddressItem personAddressItem = addressArray[i - 1];
                    data.AppendFormat( "{0}={1}&", "individual_id", $"{personAddressItem.PersonId}_{personAddressItem.PersonAliasId}_{personAddressItem.FamilyId}_{personAddressItem.LocationId}" );
                    data.AppendFormat( "{0}={1}&", "individual_first_name", personAddressItem.FirstName );
                    data.AppendFormat( "{0}={1}&", "individual_last_name", personAddressItem.LastName );
                    data.AppendFormat( "{0}={1}&", "address_line_1", personAddressItem.Street1 );
                    data.AppendFormat( "{0}={1}&", "address_line_2", personAddressItem.Street2 );
                    data.AppendFormat( "{0}={1}&", "address_city_name", personAddressItem.City );
                    data.AppendFormat( "{0}={1}&", "address_state_code", personAddressItem.State );
                    data.AppendFormat( "{0}={1}&", "address_postal_code", personAddressItem.PostalCode );
                    // data.AppendFormat( "{0}={1}&", "address_country_code", personAddressItem.Country );

                    if ( i % _batchsize == 0 || i == addressArray.Length )
                    {
                        var request = new RestRequest( $"api/files/{id}/records", Method.POST );
                        request.AddParameter( "application/x-www-form-urlencoded", data.ToString().TrimEnd( '&' ), ParameterType.RequestBody );
                        IRestResponse response = _client.Execute( request );

                        try
                        {
                            File.AppendAllText( filePath, $"{data.ToString().TrimEnd( '&' )}{Environment.NewLine}Status Code: {response.StatusCode}{Environment.NewLine}Response: {response.Content.ToStringSafe()}{Environment.NewLine}" );
                        }
                        catch { }

                        if ( response.StatusCode != HttpStatusCode.OK )
                        {
                            throw new Exception( $"Failed to upload addresses to NCOA. Status Code: {response.StatusCode}, Response: {response.Content.ToStringSafe()}" );
                        }

                        data = new StringBuilder();
                    }
                }
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Communication with NCOA server failed: Could not upload addresses to the NCOA server. Possible cause is one or more addresses are not in a valid format or invalid state code.", ex );
            }

            try
            {
                var request = new RestRequest( $"api/files/{id}/index", Method.GET );
                IRestResponse response = _client.Execute( request );
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    throw new Exception( $"Failed to upload addresses to NCOA. Status Code: {response.StatusCode}, Response: {response.Content.ToStringSafe()}" );
                }

                NcoaResponse file;
                try
                {
                    file = JsonConvert.DeserializeObject<NcoaResponse>( response.Content );
                }
                catch
                {
                    throw new Exception( $"Failed to deserialize NCOA response: {response.Content}" );
                }

                if ( file.Status != "Mapped" )
                {
                    throw new Exception( $"NCOA is not in the correct state: {file.Status}" );
                }
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Communication with NCOA server failed: Could not check upload status. Possible causes are one or more addresses are not in a valid format or invalid state code; or server is in invalid state.", ex );
            }
        }

        /// <summary>
        /// Creates the report.
        /// </summary>
        /// <param name="id">Name of the file.</param>
        public void CreateReport( string id )
        {
            try
            {
                // submit for processing
                var request = new RestRequest( $"api/files/{id}/index", Method.PATCH );
                request.AddParameter( "application/x-www-form-urlencoded", "status=submit", ParameterType.RequestBody );
                IRestResponse response = _client.Execute( request );
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
                    {
                        Content = new StringContent( response.Content )
                    } );
                }
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Communication with NCOA server failed: Could not create report. Possible causes are one or more addresses are not in a valid format or invalid state code; or server is in invalid state.", ex );
            }
        }

        /// <summary>
        /// Determines whether the report is created.
        /// </summary>
        /// <param name="id">Name of the file.</param>
        /// <returns>
        ///   <c>true</c> if the report is created; otherwise, <c>false</c>.
        /// </returns>
        public bool IsReportCreated( string id )
        {
            try
            {
                var request = new RestRequest( $"api/files/{id}/index", Method.GET );
                IRestResponse response = _client.Execute( request );
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
                    {
                        Content = new StringContent( response.Content )
                    } );
                }

                NcoaResponse file;
                try
                {
                    file = JsonConvert.DeserializeObject<NcoaResponse>( response.Content );
                }
                catch
                {
                    throw new Exception( $"Failed to deserialize NCOA response: {response.Content}" );
                }

                if ( file.Status == "Errored" )
                {
                    throw new Exception( "NCOA returned an error creating the report" );
                }

                return file.Status == "Processed";
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Communication with NCOA server failed: Could not check if report was created. Possible cause is the NCOA API server is down.", ex );
            }
        }

        /// <summary>
        /// Creates the report export.
        /// </summary>
        /// <param name="id">Name of the file.</param>
        /// <param name="exportfileid">The export file ID.</param>
        public void CreateReportExport( string id, out string exportfileid )
        {
            exportfileid = null;
            try
            {
                // submit for exporting
                var request = new RestRequest( $"api/files/{id}/index", Method.PATCH );
                request.AddParameter( "application/x-www-form-urlencoded", "status=export", ParameterType.RequestBody );
                IRestResponse response = _client.Execute( request );
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
                    {
                        Content = new StringContent( response.Content )
                    } );
                }

                try
                {
                    NcoaResponse file = JsonConvert.DeserializeObject<NcoaResponse>( response.Content );
                    exportfileid = file.Id;
                }
                catch
                {
                    throw new Exception( $"Failed to deserialize NCOA response: {response.Content}" );
                }
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Communication with NCOA server failed: Could not create export. Possible cause is the NCOA API server is down.", ex );
            }
        }

        /// <summary>
        /// Determines whether the report export is created.
        /// </summary>
        /// <param name="exportfileid">The export file ID.</param>
        /// <returns>
        ///   <c>true</c> if the report export is created; otherwise, <c>false</c>.
        /// </returns>
        public bool IsReportExportCreated( string exportfileid )
        {
            try
            {
                var request = new RestRequest( $"api/files/{exportfileid}/index", Method.GET );
                IRestResponse response = _client.Execute( request );
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
                    {
                        Content = new StringContent( response.Content )
                    } );
                }

                try
                {
                    NcoaResponse file = JsonConvert.DeserializeObject<NcoaResponse>( response.Content );
                    return file.Status == "Exported" || file.Status == "Processed";

                }
                catch
                {
                    throw new Exception( $"Failed to deserialize NCOA response: {response.Content}" );
                }
            }
            catch ( Exception ex )
            {
                throw new NoRetryAggregateException( "Communication with NCOA server failed: Could not check if the export is created. Possible cause is the NCOA API server is down.", ex );
            }
        }

        /// <summary>
        /// Downloads the export.
        /// </summary>
        /// <param name="exportfileid">The export file ID.</param>
        /// <param name="records">The records.</param>
        public void DownloadExport( string exportfileid, out List<NcoaReturnRecord> records )
        {
            records = null;

            try
            {
                DateTime dt = DateTime.Now;
                int start = 1;
                int end = 1000;
                int step = 1000;
                bool finished = false;
                records = new List<NcoaReturnRecord>();
                while ( !finished )
                {
                    var request = new RestRequest( $"api/files/{exportfileid}/records?start={start}&end={end}", Method.GET );
                    IRestResponse response = _client.Execute( request );
                    if ( response.StatusCode != HttpStatusCode.OK )
                    {
                        throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
                        {
                            Content = new StringContent( response.Content )
                        } );
                    }

                    Dictionary<string, object> obj = null;
                    try
                    {
                        obj = JObject.Parse( response.Content ).ToObject<Dictionary<string, object>>();

                        var recordsjson = ( string ) obj["Records"].ToString();
                        List<NcoaReturnRecord> instanceRecords = JsonConvert.DeserializeObject<List<NcoaReturnRecord>>( recordsjson, new JsonSerializerSettings
                        {
                            MissingMemberHandling = MissingMemberHandling.Error
                        } );

                        if ( instanceRecords == null )
                        {
                            instanceRecords = new List<NcoaReturnRecord>();
                        }

                        instanceRecords.ForEach( r => r.NcoaRunDateTime = dt );
                        records.AddRange( instanceRecords );
                        if ( instanceRecords.Count < step )
                        {
                            finished = true;
                        }
                        else
                        {
                            start += step;
                            end += step;
                        }
                    }
                    catch ( Exception ex )
                    {
                        if ( obj != null && obj.ContainsKey( "error" ) )
                        {
                            throw new Exception( $"NCOA error response: {obj["error"]}" );
                        }
                        else
                        {
                            throw new AggregateException( $"Failed to deserialize NCOA response: {response.Content}", ex );
                        }
                    }
                }

                // NCOA return two entries for each move. One for the new address, and one for the previous address. Remove the old address because it is not required.
                var movedRecords = records.Where( r => r.RecordType == "C" && r.MatchFlag == "M" ).ToList(); // Find all the moved addresses with a current address
                records.RemoveAll( r => r.RecordType == "H" && movedRecords.Any( m => m.InputIndividualId == r.InputIndividualId ) ); // Delete all the moved addresses's previous addresses

            }
            catch ( Exception ex )
            {
                throw new NoRetryAggregateException( "Communication with NCOA server failed: Could not download export. Possible cause is the NCOA API server is down.", ex );
            }
        }

        /// <summary>
        /// Saves the records.
        /// </summary>
        /// <param name="records">The records.</param>
        /// <param name="fileName">Name of the file.</param>
        public void SaveRecords( List<NcoaReturnRecord> records, string fileName )
        {
            DataTable dtRecords = null;
            string recordsjson = JsonConvert.SerializeObject( records );
            dtRecords = (DataTable)JsonConvert.DeserializeObject( recordsjson, ( typeof( DataTable ) ) );
            StringBuilder sb = new StringBuilder();
            IEnumerable<string> columnNames = dtRecords.Columns.Cast<DataColumn>().Select( column => column.ColumnName );
            sb.AppendLine( string.Join( ",", columnNames ) );
            foreach ( DataRow row in dtRecords.Rows )
            {
                IEnumerable<string> fields = row.ItemArray.Select( field => string.Concat( "\"", field.ToString().Replace( "\"", "\"\"" ), "\"" ) );
                sb.AppendLine( string.Join( ",", fields ) );
            }

            if ( System.IO.File.Exists( fileName ) )
            {
                System.IO.File.Delete( fileName );
            }

            System.IO.File.WriteAllText( fileName, sb.ToString() );
        }
    }
}