using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestSharp;
using System.Configuration;
using System.IO;


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

        public bool AuthenicateUser( string username, string password )
        {
            string errorResponse = string.Empty;
            return AuthenicateUser( username, password, out  errorResponse );
        }
        
        public bool AuthenicateUser( string username, string password, out string errorResponse )
        {
            errorResponse = string.Empty;

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

        public static string GetOrganizationKey()
        {
            var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
            return globalAttributes.GetValue( "StoreOrganizationKey" );
        }

    }
}
