﻿// <copyright>
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
        public OrganizationService() :base()
        {}

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

            // setup REST call
            var client = new RestClient( _rockStoreUrl );
            client.Timeout = _clientTimeout;

            string encodedUserName = HttpUtility.UrlEncode( Convert.ToBase64String( Encoding.UTF8.GetBytes( username ) ) );
            string encodedPassword = HttpUtility.UrlEncode( Convert.ToBase64String( Encoding.UTF8.GetBytes( password ) ) );
            string requestUrl = string.Format( "api/Store/RetrieveOrganizations/{0}/{1}", encodedUserName, encodedPassword );
            var request = new RestRequest( requestUrl, Method.GET );

            // deserialize to list of packages
            var response = client.Execute<List<Organization>>( request );

            if ( response.ResponseStatus == ResponseStatus.Completed )
            {
                return response.Data;
            }
            else
            {
                errorResponse = response.ErrorMessage;
                return new List<Organization>();
            }
        }
    }
}
