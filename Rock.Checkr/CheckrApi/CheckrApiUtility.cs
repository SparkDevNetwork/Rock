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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using Rock.Web.Cache;
using Rock.Checkr.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Checkr.CheckrApi
{
    internal static class CheckrApiUtility
    {
        #region Utilities        
        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static List<AttributeValue> GetSettings( RockContext rockContext )
        {
            var checkrEntityType = EntityTypeCache.Get( typeof( Rock.Checkr.Checkr ) );
            if ( checkrEntityType != null )
            {
                var service = new AttributeValueService( rockContext );
                return service.Queryable( "Attribute" )
                    .Where( v => v.Attribute.EntityTypeId == checkrEntityType.Id )
                    .ToList();
            }

            return null;
        }

        /// <summary>
        /// Gets the setting value.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private static string GetSettingValue( List<AttributeValue> values, string key, bool encryptedValue = false )
        {
            string value = values
                .Where( v => v.AttributeKey == key )
                .Select( v => v.Value )
                .FirstOrDefault();
            if ( encryptedValue && !string.IsNullOrWhiteSpace( value ) )
            {
                try { value = Encryption.DecryptString( value ); }
                catch { }
            }

            return value;
        }

        /// <summary>
        /// Return a rest client.
        /// </summary>
        /// <returns>The rest client.</returns>
        private static RestClient RestClient()
        {
            string token = null;
            var restClient = new RestClient( CheckrConstants.CHECKR_APISERVER );
            using ( RockContext rockContext = new RockContext() )
            {
                var settings = GetSettings( rockContext );
                if ( settings != null )
                {
                    token = GetSettingValue( settings, "AccessToken", true );
                }
            }

            restClient.Authenticator = new HttpBasicAuthenticator( token, string.Empty );
            return restClient;
        }

        /// <summary>
        /// RestClient request to string for debugging purposes.
        /// </summary>
        /// <param name="restClient">The rest client.</param>
        /// <param name="restRequest">The rest request.</param>
        /// <returns>The RestClient Request in string format.</returns>
        // https://stackoverflow.com/questions/15683858/restsharp-print-raw-request-and-response-headers
        private static string RequestToString( RestClient restClient, RestRequest restRequest )
        {
            var requestToLog = new
            {
                resource = restRequest.Resource,
                // Parameters are custom anonymous objects in order to have the parameter type as a nice string
                // otherwise it will just show the enum value
                parameters = restRequest.Parameters.Select( parameter => new
                {
                    name = parameter.Name,
                    value = parameter.Value,
                    type = parameter.Type.ToString()
                } ),
                // ToString() here to have the method as a nice string otherwise it will just show the enum value
                method = restRequest.Method.ToString(),
                // This will generate the actual Uri used in the request
                uri = restClient.BuildUri( restRequest ),
            };
            return JsonConvert.SerializeObject( requestToLog );
        }

        /// <summary>
        /// RestClient response to string for debugging purposes.
        /// </summary>
        /// <param name="restResponse">The rest response.</param>
        /// <returns>The RestClient response in string format.</returns>
        // https://stackoverflow.com/questions/15683858/restsharp-print-raw-request-and-response-headers
        private static string ResponseToString( IRestResponse restResponse )
        {
            var responseToLog = new
            {
                statusCode = restResponse.StatusCode,
                content = restResponse.Content,
                headers = restResponse.Headers,
                // The Uri that actually responded (could be different from the requestUri if a redirection occurred)
                responseUri = restResponse.ResponseUri,
                errorMessage = restResponse.ErrorMessage,
            };

            return JsonConvert.SerializeObject( responseToLog );
        }
        #endregion
        #region Public Methods
        /// <summary>
        /// Gets the packages.
        /// </summary>
        /// <param name="getPackagesResponse">The get packages response.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>True/False value of whether the request was successfully sent or not.</returns>
        internal static bool GetPackages( out GetPackagesResponse getPackagesResponse, List<string> errorMessages )
        {
            getPackagesResponse = null;
            RestClient restClient = RestClient();
            RestRequest restRequest = new RestRequest( CheckrConstants.CHECKR_PACKAGES_URL );
            IRestResponse restResponse = restClient.Execute( restRequest );

            if ( restResponse.StatusCode == HttpStatusCode.Unauthorized )
            {
                errorMessages.Add( "Failed to authorize Checkr. Please confirm your access token." );
                return false;
            }

            if ( restResponse.StatusCode != HttpStatusCode.OK )
            {
                errorMessages.Add( "Failed to get Checkr Packages: " + restResponse.Content );
                return false;
            }

            getPackagesResponse = JsonConvert.DeserializeObject<GetPackagesResponse>( restResponse.Content );
            if ( getPackagesResponse == null )
            {
                errorMessages.Add( "Get Packages is not valid: " + restResponse.Content );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates the candidate.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="createCandidateResponse">The create candidate response.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>True/False value of whether the request was successfully sent or not.</returns>
        internal static bool CreateCandidate( Person person, out CreateCandidateResponse createCandidateResponse, List<string> errorMessages )
        {
            createCandidateResponse = null;
            RestClient restClient = RestClient();
            RestRequest restRequest = new RestRequest( CheckrConstants.CHECKR_CANDIDATES_URL, Method.POST );
            restRequest.AddParameter( "first_name", person.FirstName );
            restRequest.AddParameter( "middle_name", person.MiddleName );
            restRequest.AddParameter( "no_middle_name", person.MiddleName.IsNullOrWhiteSpace() );
            restRequest.AddParameter( "last_name", person.LastName );
            restRequest.AddParameter( "email", person.Email );

            // request = RequestToString( restClient, restRequest ); // For debugging
            IRestResponse restResponse = restClient.Execute( restRequest );
            // response = ResponseToString( restResponse ); // For debugging

            if ( restResponse.StatusCode == HttpStatusCode.Unauthorized )
            {
                errorMessages.Add( "Invalid Checkr access token. To Re-authenticate go to Admin Tools > System Settings > Checkr. Click edit to change your access token." );
                return false;
            }

            if ( restResponse.StatusCode != HttpStatusCode.Created )
            {
                errorMessages.Add( "Failed to create Checkr Candidate: " + restResponse.Content );
                return false;
            }

            createCandidateResponse = JsonConvert.DeserializeObject<CreateCandidateResponse>( restResponse.Content );
            if ( createCandidateResponse == null )
            {
                errorMessages.Add( "Create Candidate Response is not valid: " + restResponse.Content );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates the invitation.
        /// </summary>
        /// <param name="candidateId">The candidate identifier.</param>
        /// <param name="package">The package.</param>
        /// <param name="createInvitationResponse">The create invitation response.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>True/False value of whether the request was successfully sent or not.</returns>
        internal static bool CreateInvitation( string candidateId, string package, out CreateInvitationResponse createInvitationResponse, List<string> errorMessages )
        {
            createInvitationResponse = null;
            RestClient restClient = RestClient();
            RestRequest restRequest = new RestRequest( CheckrConstants.CHECKR_INVITATIONS_URL, Method.POST );
            restRequest.AddParameter( "candidate_id", candidateId );
            restRequest.AddParameter( "package", package );

            // request = RequestToString( restClient, restRequest ); // For debugging
            IRestResponse restResponse = restClient.Execute( restRequest );
            // response = ResponseToString( restResponse ); // For debugging

            if ( restResponse.StatusCode == HttpStatusCode.Unauthorized )
            {
                errorMessages.Add( "Invalid Checkr access token. To Re-authenticate go to Admin Tools > System Settings > Checkr. Click edit to change your access token." );
                return false;
            }

            if ( restResponse.StatusCode != HttpStatusCode.Created )
            {
                errorMessages.Add( "Failed to create Checkr Candidate: " + restResponse.Content );
                return false;
            }

            createInvitationResponse = JsonConvert.DeserializeObject<CreateInvitationResponse>( restResponse.Content );
            if ( createInvitationResponse == null )
            {
                errorMessages.Add( "Create Invitation Response is not valid: " + restResponse.Content );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the report.
        /// </summary>
        /// <param name="reportId">The report identifier.</param>
        /// <param name="getReportResponse">The get report response.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>True/False value of whether the request was successfully sent or not.</returns>
        internal static bool GetReport( string reportId, out GetReportResponse getReportResponse, List<string> errorMessages )
        {
            getReportResponse = null;
            RestClient restClient = RestClient();
            RestRequest restRequest = new RestRequest( CheckrConstants.CHECKR_REPORT_URL + "/" + reportId );
            IRestResponse restResponse = restClient.Execute( restRequest );

            if ( restResponse.StatusCode == HttpStatusCode.Unauthorized )
            {
                errorMessages.Add( "Invalid Checkr access token. To Re-authenticate go to Admin Tools > System Settings > Checkr. Click edit to change your access token." );
                return false;
            }

            if ( restResponse.StatusCode != HttpStatusCode.OK )
            {
                errorMessages.Add( "Failed to get Checkr Report: " + restResponse.Content );
                return false;
            }

            getReportResponse = JsonConvert.DeserializeObject<GetReportResponse>( restResponse.Content );
            if ( getReportResponse == null )
            {
                errorMessages.Add( "Get Report is not valid: " + restResponse.Content );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the document.
        /// </summary>
        /// <param name="documentId">The document identifier.</param>
        /// <param name="getDocumentResponse">The get document response.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>True/False value of whether the request was successfully sent or not.</returns>
        internal static bool GetDocument( string documentId, out GetDocumentResponse getDocumentResponse, List<string> errorMessages )
        {
            getDocumentResponse = null;
            RestClient restClient = RestClient();
            RestRequest restRequest = new RestRequest( CheckrConstants.CHECKR_DOCUMENT_URL + "/" + documentId );
            IRestResponse restResponse = restClient.Execute( restRequest );

            if ( restResponse.StatusCode == HttpStatusCode.Unauthorized )
            {
                errorMessages.Add( "Invalid Checkr access token. To Re-authenticate go to Admin Tools > System Settings > Checkr. Click edit to change your access token." );
                return false;
            }

            if ( restResponse.StatusCode != HttpStatusCode.OK )
            {
                errorMessages.Add( "Failed to get Checkr Document: " + restResponse.Content );
                return false;
            }

            getDocumentResponse = JsonConvert.DeserializeObject<GetDocumentResponse>( restResponse.Content );
            if ( getDocumentResponse == null )
            {
                errorMessages.Add( "Get Documentation is not valid: " + restResponse.Content );
                return false;
            }

            return true;
        }
        #endregion
    }
}
