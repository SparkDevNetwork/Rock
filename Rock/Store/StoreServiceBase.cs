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
using System.Configuration;
using System.Text;
using System.Web;
using RestSharp;

namespace Rock.Store
{
    /// <summary>
    /// Base service class for store services
    /// </summary>
    public class StoreServiceBase
    {
        /// <summary>
        /// Internal variable to store the URL to the store.
        /// </summary>
        protected string _rockStoreUrl = string.Empty;

        /// <summary>
        /// The Client Timeout
        /// </summary>
        protected int _clientTimeout = 12000;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreServiceBase"/> class.
        /// </summary>
        public StoreServiceBase()
        {
            // set configuration variables
            _rockStoreUrl = ConfigurationManager.AppSettings["RockStoreUrl"];
        }

        /// <summary>
        /// Executes the rest get request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resource">The resource.</param>
        /// <returns></returns>
        public IRestResponse<T> ExecuteRestGetRequest<T>( string resource ) where T : new()
        {
            return ExecuteRestGetRequest<T>( resource, null );
        }

        /// <summary>
        /// Executes the rest get request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="queryParameters">The query parameters.</param>
        /// <returns></returns>
        public IRestResponse<T> ExecuteRestGetRequest<T>( string resource, Dictionary<string, List<string>> queryParameters ) where T : new()
        {
            var client = new RestClient( _rockStoreUrl )
            {
                Timeout = _clientTimeout
            };

            var request = new RestRequest
            {
                Method = Method.GET,
                Resource = resource
            };

            if ( queryParameters != null )
            {
                foreach ( var keyValuePair in queryParameters )
                {
                    foreach ( var item in keyValuePair.Value )
                    {
                        request.AddParameter( keyValuePair.Key, item, ParameterType.QueryString );
                    }
                }
            }

            return client.Execute<T>( request );
        }

        /// <summary>
        /// Gets the organization key.
        /// </summary>
        /// <returns></returns>
        public static string GetOrganizationKey()
        {
            string encryptedStoreKey = Rock.Web.SystemSettings.GetValue( "StoreOrganizationKey" );

            string decryptedStoreKey = Rock.Security.Encryption.DecryptString( encryptedStoreKey );

            if ( decryptedStoreKey == null && encryptedStoreKey.Length < 90 )
            {
                // if the decryption fails, it could be that the StoreOrganizationKey isn't encrypted, so encrypt and store it again 
                decryptedStoreKey = encryptedStoreKey;
                SetOrganizationKey( decryptedStoreKey );
            }

            if ( decryptedStoreKey == null || decryptedStoreKey.Length > 90 )
            {
                Model.ExceptionLogService.LogException( "The Rock 'Store Key' for the organization is too long. This typically means it was copied from another system and is unable to be decrypted by this system." );
                return string.Empty;
            }

            return decryptedStoreKey;
        }

        /// <summary>
        /// Gets the encoded organization key.
        /// </summary>
        /// <returns></returns>
        public static string GetEncodedOrganizationKey()
        {
            var organizationKey = StoreService.GetOrganizationKey();
            return HttpUtility.UrlEncode( Convert.ToBase64String( Encoding.UTF8.GetBytes( organizationKey ) ) );
        }

        /// <summary>
        /// Sets the organization key.
        /// </summary>
        /// <param name="storeKey">The store key.</param>
        public static void SetOrganizationKey( string storeKey )
        {
            Rock.Web.SystemSettings.SetValue( "StoreOrganizationKey", Rock.Security.Encryption.EncryptString( storeKey ) );
        }

        /// <summary>
        /// Revokes the organization key.
        /// </summary>
        public static void RevokeOrganizationKey()
        {
            Rock.Web.SystemSettings.SetValue( "StoreOrganizationKey", null );
        }
    }
}
