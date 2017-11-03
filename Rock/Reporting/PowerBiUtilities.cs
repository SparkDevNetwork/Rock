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
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;

using Rock.Attribute;
using Rock.Web.Cache;

namespace Rock.Reporting
{
    /// <summary>
    /// Power BI Utilities Class
    /// </summary>
    public static class PowerBiUtilities
    {
        private static readonly string _authorityUri = "https://login.windows.net/common/oauth2/authorize/";
        private static readonly string _resourceUri = "https://analysis.windows.net/powerbi/api";
        private static readonly string _baseUri = "https://api.powerbi.com/v1.0/myorg/";

        /// <summary>
        /// Gets the authority URI.
        /// </summary>
        /// <value>
        /// The authority URI.
        /// </value>
        public static string AuthorityUri
        {
            get
            {
                return _authorityUri;
            }
        }

        /// <summary>
        /// Gets the resource URI.
        /// </summary>
        /// <value>
        /// The resource URI.
        /// </value>
        public static string ResourceUri
        {
            get
            {
                return _resourceUri;
            }
        }

        /// <summary>
        /// Gets the base URI.
        /// </summary>
        /// <value>
        /// The base URI.
        /// </value>
        public static string BaseUri
        {
            get
            {
                return _baseUri;
            }
        }

        /// <summary>
        /// Authenticates the account.
        /// </summary>
        /// <param name="accountValueGuid">The account value unique identifier.</param>
        /// <param name="returnUrl">The return URL.</param>
        public static void AuthenticateAccount( Guid accountValueGuid, string returnUrl )
        {
            var biAccountValue = DefinedValueCache.Read( accountValueGuid );

            AuthenticateAccount( biAccountValue, returnUrl );
        }

        /// <summary>
        /// Authenticates the account.
        /// </summary>
        /// <param name="biAccountValue">The bi account value.</param>
        /// <param name="returnUrl">The return URL.</param>
        private static void AuthenticateAccount( DefinedValueCache biAccountValue, string returnUrl )
        {
            if ( biAccountValue != null )
            {
                var clientId = biAccountValue.AttributeValues.Where( v => v.Key == "ClientId" ).Select( v => v.Value.Value ).FirstOrDefault();
                var redirectUrl = biAccountValue.AttributeValues.Where( v => v.Key == "RedirectUrl" ).Select( v => v.Value.Value ).FirstOrDefault();

                HttpContext.Current.Session["PowerBiAccountValueId"] = biAccountValue.Id.ToString();
                HttpContext.Current.Session["PowerBiRedirectUri"] = redirectUrl;
                HttpContext.Current.Session["PowerBiRockReturnUrl"] = returnUrl;

                // now that everything is saved redirect for Power BI authenication
                var @params = new NameValueCollection
                {
                    // Azure AD will return an authorization code -see the Redirect class to see how "code" is used to AcquireTokenByAuthorizationCode
                    {"response_type", "code"},

                    // client ID is used by the application to identify themselves to the users that they are requesting permissions from. 
                    {"client_id", clientId},

                    // resource uri to the Power BI resource to be authorized
                    {"resource", _resourceUri},

                    // after user authenticates, Azure AD will redirect back to the web app
                    {"redirect_uri", redirectUrl }
                };

                // create sign-in query string
                var queryString = HttpUtility.ParseQueryString( string.Empty );
                queryString.Add( @params );

                // redirect authority - authority Uri is an Azure resource that takes a client id to get an access token
                string authorityUri = _authorityUri;
                var authUri = String.Format( "{0}?{1}", authorityUri, queryString );
                HttpContext.Current.Response.Redirect( authUri );
            }
        }

        /// <summary>
        /// Creates the account.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="accountDescription">The account description.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <param name="redirectUrl">The redirect URL.</param>
        /// <param name="returnUrl">The return URL.</param>
        public static void CreateAccount( string accountName, string accountDescription, string clientId, string clientSecret, string redirectUrl, string returnUrl )
        {
            // save all this stuff to Session so that the PowerBIAuth webhook has what it needs to process
            HttpContext.Current.Session["PowerBiAccountValueId"] = "0";
            HttpContext.Current.Session["PowerBiAccountName"] = accountName;
            HttpContext.Current.Session["PowerBiAccountDescription"] = accountDescription;
            HttpContext.Current.Session["PowerBiClientId"] = clientId;
            HttpContext.Current.Session["PowerBiClientSecret"] = clientSecret;
            HttpContext.Current.Session["PowerBiRedirectUri"] = redirectUrl;
            HttpContext.Current.Session["PowerBiRockReturnUrl"] = returnUrl;

            // now that everything is saved redirect for Power BI authenication
            var @params = new NameValueCollection
            {
                // Azure AD will return an authorization code -see the Redirect class to see how "code" is used to AcquireTokenByAuthorizationCode
                {"response_type", "code"},

                // client ID is used by the application to identify themselves to the users that they are requesting permissions from. 
                {"client_id", clientId},

                // resource uri to the Power BI resource to be authorized
                {"resource", _resourceUri},

                // after user authenticates, Azure AD will redirect back to the web app
                {"redirect_uri", redirectUrl }
            };

            // create sign-in query string
            var queryString = HttpUtility.ParseQueryString( string.Empty );
            queryString.Add( @params );

            // redirect authority - authority Uri is an Azure resource that takes a client id to get an access token
            string authorityUri = _authorityUri;
            var authUri = String.Format( "{0}?{1}", authorityUri, queryString );
            HttpContext.Current.Response.Redirect( authUri );
        }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <param name="biAccountValue">The bi account value.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private static string GetAccessToken( DefinedValueCache biAccountValue, out string message )
        {
            message = string.Empty;
            string accessCode = string.Empty;

            if ( biAccountValue != null )
            {
                var refreshToken = biAccountValue.AttributeValues.Where( v => v.Key == "RefreshToken" ).Select( v => v.Value.Value ).FirstOrDefault();
                var clientId = biAccountValue.AttributeValues.Where( v => v.Key == "ClientId" ).Select( v => v.Value.Value ).FirstOrDefault();
                var clientSecret = biAccountValue.AttributeValues.Where( v => v.Key == "ClientSecret" ).Select( v => v.Value.Value ).FirstOrDefault();

                try
                {
                    TokenCache TC = new TokenCache();
                    AuthenticationContext AC = new AuthenticationContext( PowerBiUtilities.AuthorityUri, TC );
                    var clientCredential = new ClientCredential( clientId, clientSecret );
                    AuthenticationResult AR = AC.AcquireTokenByRefreshToken( refreshToken, clientCredential );

                    accessCode = AR.AccessToken;

                    // store the new refresh token
                    var refreshTokenAttribute = biAccountValue.Attributes
                        .Where( a => a.Value.Key == "RefreshToken" )
                        .Select( a => a.Value )
                        .FirstOrDefault();

                    Helper.SaveAttributeValue( biAccountValue, refreshTokenAttribute, AR.RefreshToken, new Rock.Data.RockContext() );
                }
                catch ( Exception ex )
                {
                    message = ex.Message;
                }
            }
            else
            {
                message = "Could not find saved BI account.";
            }

            return accessCode;
        }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <param name="accountValueGuid">The account value unique identifier.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static string GetAccessToken( Guid accountValueGuid, out string message )
        {
            var biAccountValue = DefinedValueCache.Read( accountValueGuid );

            return GetAccessToken( biAccountValue, out message );
        }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <param name="accountValueGuid">The account value unique identifier.</param>
        /// <returns></returns>
        public static string GetAccessToken( Guid accountValueGuid )
        {
            string message;
            return GetAccessToken( accountValueGuid, out message );
        }

        /// <summary>
        /// Gets the groups
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static List<PBIGroup> GetGroups( string accessToken, out string message )
        {
            message = string.Empty;

            if ( !string.IsNullOrWhiteSpace( accessToken ) )
            {
                string responseContent = string.Empty;

                try
                {
                    // configure groups request
                    System.Net.WebRequest request;
                    request = System.Net.WebRequest.Create( $"{_baseUri}groups" ) as System.Net.HttpWebRequest;

                    request.Method = "GET";
                    request.ContentLength = 0;
                    request.Headers.Add( "Authorization", String.Format( "Bearer {0}", accessToken ) );

                    // get response from request.GetResponse()
                    using ( var response = request.GetResponse() as System.Net.HttpWebResponse )
                    {
                        // get reader from response stream
                        using ( var reader = new System.IO.StreamReader( response.GetResponseStream() ) )
                        {
                            responseContent = reader.ReadToEnd();

                            // deserialize JSON string
                            PBIGroups groups = JsonConvert.DeserializeObject<PBIGroups>( responseContent );

                            return groups.value.ToList();
                        }
                    }
                }
                catch ( Exception ex )
                {
                    message = ex.Message;
                }
            }
            else
            {
                message = "Invalid access token was used to retrieve group list.";
            }

            return new List<PBIGroup>();
        }

        /// <summary>
        /// Gets the reports.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static List<PBIReport> GetReports( string accessToken, string groupId, out string message )
        {
            message = string.Empty;

            if ( !string.IsNullOrWhiteSpace( accessToken ) )
            {
                string responseContent = string.Empty;

                try
                {
                    // configure reports request
                    System.Net.WebRequest request;
                    if ( groupId.IsNotNullOrWhitespace() )
                    {
                        request = System.Net.WebRequest.Create( $"{_baseUri}groups/{groupId}/reports" ) as System.Net.HttpWebRequest;
                    }
                    else
                    {
                        request = System.Net.WebRequest.Create( $"{_baseUri}reports" ) as System.Net.HttpWebRequest;
                    }

                    request.Method = "GET";
                    request.ContentLength = 0;
                    request.Headers.Add( "Authorization", String.Format( "Bearer {0}", accessToken ) );

                    // get response from request.GetResponse()
                    using ( var response = request.GetResponse() as System.Net.HttpWebResponse )
                    {
                        // get reader from response stream
                        using ( var reader = new System.IO.StreamReader( response.GetResponseStream() ) )
                        {
                            responseContent = reader.ReadToEnd();

                            // deserialize JSON string
                            PBIReports PBIReports = JsonConvert.DeserializeObject<PBIReports>( responseContent );

                            return PBIReports.value.ToList();
                        }
                    }
                }
                catch ( Exception ex )
                {
                    message = ex.Message;
                }
            }
            else
            {
                message = "Invalid access token was used to retrieve report list.";
            }

            return new List<PBIReport>();
        }

        /// <summary>
        /// Gets the groups.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns></returns>
        public static List<PBIGroup> GetGroups( string accessToken )
        {
            string message;
            return GetGroups( accessToken, out message );
        }

        /// <summary>
        /// Gets the reports.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <returns></returns>
        public static List<PBIReport> GetReports( string accessToken, string groupId )
        {
            string message;
            return GetReports( accessToken, groupId, out message );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PBIReports
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public PBIReport[] value { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PBIReport
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string name { get; set; }

        /// <summary>
        /// Gets or sets the web URL.
        /// </summary>
        /// <value>
        /// The web URL.
        /// </value>
        public string webUrl { get; set; }

        /// <summary>
        /// Gets or sets the embed URL.
        /// </summary>
        /// <value>
        /// The embed URL.
        /// </value>
        public string embedUrl { get; set; }

        /// <summary>
        /// Gets or sets the dataset identifier.
        /// </summary>
        /// <value>
        /// The dataset identifier.
        /// </value>
        public string datasetId { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PBIGroups
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public PBIGroup[] value { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PBIGroup
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly { get; set; }
    }

}
