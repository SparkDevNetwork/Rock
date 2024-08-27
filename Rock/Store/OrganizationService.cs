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
using System.Text;
using System.Web;

using RestSharp;


namespace Rock.Store
{
    /// <summary>
    /// Service class for the store category model.
    /// </summary>
    public class OrganizationService : StoreServiceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationService"/> class.
        /// </summary>
        public OrganizationService() : base()
        { }

        /// <summary>
        /// Gets the organizations.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public IEnumerable<Organization> GetOrganizations( string username, string password )
        {
            string errorResponse = string.Empty;
            return GetOrganizations( username, password, out errorResponse );
        }

        /// <summary>
        /// Gets the organizations.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="errorResponse">The error response.</param>
        /// <returns></returns>
        public IEnumerable<Organization> GetOrganizations( string username, string password, out string errorResponse )
        {
            errorResponse = string.Empty;

            string encodedUserName = HttpUtility.UrlEncode( Convert.ToBase64String( Encoding.UTF8.GetBytes( username ) ) );
            string encodedPassword = HttpUtility.UrlEncode( Convert.ToBase64String( Encoding.UTF8.GetBytes( password ) ) );

            var response = ExecuteRestGetRequest<List<Organization>>( $"api/Store/RetrieveOrganizations/{encodedUserName}/{encodedPassword}" );

            if ( response.ResponseStatus == ResponseStatus.Completed && response.Data != null )
            {
                return response.Data;
            }
            else
            {
                errorResponse = response.ErrorMessage;
                return new List<Organization>();
            }
        }

        /// <summary>
        /// Gets the organization.
        /// </summary>
        /// <param name="organizationKey">The organization key.</param>
        /// <returns></returns>
        public StoreApiResult<Organization> GetOrganization( string organizationKey )
        {
            string encodedOrganizationKey = HttpUtility.UrlEncode( Convert.ToBase64String( Encoding.UTF8.GetBytes( organizationKey ) ) );

            var response = ExecuteRestGetRequest<Organization>( $"api/Store/RetrieveOrganization/{encodedOrganizationKey}" );

            if ( response.ResponseStatus == ResponseStatus.Completed )
            {
                return new StoreApiResult<Organization>
                {
                    Result = response.Data
                };
            }

            return new StoreApiResult<Organization>
            {
                ErrorResponse = response.ErrorMessage
            };
        }

        /// <summary>
        /// Sets the size of the organization.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="organizationKey">The organization key.</param>
        /// <param name="averageWeeklyAttendance">The average weekly attendance.</param>
        /// <returns></returns>
        public StoreApiResult<Organization> SetOrganizationSize( string username, string password, string organizationKey, int averageWeeklyAttendance )
        {
            // setup REST call
            var client = new RestClient( _rockStoreUrl );
            client.Timeout = _clientTimeout;

            string encodedOrganizationKey = HttpUtility.UrlEncode( Convert.ToBase64String( Encoding.UTF8.GetBytes( organizationKey ) ) );
            string encodedUserName = HttpUtility.UrlEncode( Convert.ToBase64String( Encoding.UTF8.GetBytes( username ) ) );
            string encodedPassword = HttpUtility.UrlEncode( Convert.ToBase64String( Encoding.UTF8.GetBytes( password ) ) );
            string requestUrl = string.Format( $"api/Store/DefineOrganizationSize" );

            var body = new
            {
                organizationKey = encodedOrganizationKey,
                username = encodedUserName,
                password = encodedPassword,
                averageWeeklyAttendance
            };

            var request = new RestRequest( requestUrl, Method.POST );
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody( body );

            // deserialize to list of packages
            var response = client.Execute<Organization>( request );

            if ( response.ResponseStatus == ResponseStatus.Completed )
            {
                return new StoreApiResult<Organization>
                {
                    Result = response.Data
                };
            }

            return new StoreApiResult<Organization>
            {
                ErrorResponse = response.ErrorMessage
            };
        }
    }
}
