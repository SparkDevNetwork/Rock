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
using System.Text;
using System.Web;

using RestSharp;


namespace Rock.Store
{
    /// <summary>
    /// Service class for the store category model.
    /// </summary>
    public class StoreService : StoreServiceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StoreService"/> class.
        /// </summary>
        public StoreService() : base()
        { }

        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public bool AuthenicateUser( string username, string password )
        {
            string errorResponse = string.Empty;
            return AuthenicateUser( username, password, out errorResponse );
        }

        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="errorResponse">The error response.</param>
        /// <returns></returns>
        public bool AuthenicateUser( string username, string password, out string errorResponse )
        {
            errorResponse = string.Empty;

            string encodedUserName = HttpUtility.UrlEncode( Convert.ToBase64String( Encoding.UTF8.GetBytes( username ) ) );
            string encodedPassword = HttpUtility.UrlEncode( Convert.ToBase64String( Encoding.UTF8.GetBytes( password ) ) );

            var response = ExecuteRestGetRequest<bool>( $"api/Store/AuthenicateUser/{encodedUserName}/{encodedPassword}" );

            if ( response.ResponseStatus == ResponseStatus.Completed )
            {
                return response.Data;
            }
            else
            {
                errorResponse = response.ErrorMessage;
                return false;
            }
        }

        /// <summary>
        /// Purchases the specified username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="errorResponse">The error response.</param>
        /// <returns></returns>
        public PurchaseResponse Purchase( string username, string password, int packageId, out string errorResponse )
        {
            errorResponse = string.Empty;

            // setup REST call
            var client = new RestClient( _rockStoreUrl );
            client.Timeout = _clientTimeout;

            string organizationKey = GetOrganizationKey();
            string encodedUserName = HttpUtility.UrlEncode( Convert.ToBase64String( Encoding.UTF8.GetBytes( username ) ) );
            string encodedPassword = HttpUtility.UrlEncode( Convert.ToBase64String( Encoding.UTF8.GetBytes( password ) ) );

            string requestUrl = string.Format( $"api/Store/ProcessPurchase" );

            var body = new
            {
                organizationKey,
                username = encodedUserName,
                password = encodedPassword,
                packageId
            };

            var request = new RestRequest( requestUrl, Method.POST );
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody( body );

            // deserialize to list of packages
            var response = client.Execute<PurchaseResponse>( request );

            if ( response.ResponseStatus == ResponseStatus.Completed )
            {
                return response.Data;
            }
            else
            {
                errorResponse = response.ErrorMessage;
                return null;
            }
        }

        /// <summary>
        /// Returns true if the Organization has a StoreOrganizationKey
        /// </summary>
        /// <returns></returns>
        public static bool OrganizationIsConfigured()
        {
            var storeKey = StoreService.GetOrganizationKey();

            if ( string.IsNullOrWhiteSpace( storeKey ) )
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
