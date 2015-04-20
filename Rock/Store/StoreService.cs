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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestSharp;
using System.Configuration;
using System.IO;
using System.Web;


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
        public StoreService() :base()
        {}

        /// <summary>
        /// Authenicates the user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public bool AuthenicateUser( string username, string password )
        {
            string errorResponse = string.Empty;
            return AuthenicateUser( username, password, out  errorResponse );
        }

        /// <summary>
        /// Authenicates the user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="errorResponse">The error response.</param>
        /// <returns></returns>
        public bool AuthenicateUser( string username, string password, out string errorResponse )
        {
            errorResponse = string.Empty;

            // url encode the password
            password = HttpUtility.UrlEncode( password );

            // setup REST call
            var client = new RestClient( _rockStoreUrl );
            client.Timeout = _clientTimeout;
            string requestUrl = string.Format( "api/Store/AuthenicateUser/{0}/{1}", username, password );
            var request = new RestRequest( requestUrl, Method.GET );

            // deserialize to list of packages
            var response = client.Execute<bool>( request );

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
        /// <returns></returns>
        public PurchaseResponse Purchase( string username, string password, int packageId )
        {
            string errorResponse = string.Empty;
            return Purchase( username, password, packageId, out errorResponse );
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

            // get organization key
            string organizationKey = GetOrganizationKey();

            // setup REST call
            var client = new RestClient( _rockStoreUrl );
            client.Timeout = _clientTimeout;
            string requestUrl = string.Format( "api/Store/Purchase/{0}/{1}/{2}/{3}", username, password, organizationKey, packageId.ToString() );
            var request = new RestRequest( requestUrl, Method.GET );

            // deserialize to list of packages
            var response = client.Execute<PurchaseResponse>( request );

            if ( response.ResponseStatus == ResponseStatus.Completed )
            {
                return response.Data;
            }
            else
            {
                errorResponse = response.ErrorMessage;
                return new PurchaseResponse();
            }
        }

        /// <summary>
        /// Organizations the is configured.
        /// </summary>
        /// <returns></returns>
        public static bool OrganizationIsConfigured()
        {
            var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
            string storeKey = globalAttributes.GetValue( "StoreOrganizationKey" );

            if ( string.IsNullOrWhiteSpace( storeKey ) )
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the organization key.
        /// </summary>
        /// <returns></returns>
        public static string GetOrganizationKey()
        {
            var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
            return globalAttributes.GetValue( "StoreOrganizationKey" );
        }

    }
}
