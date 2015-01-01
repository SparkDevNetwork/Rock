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
    public class OrganizationService : StoreServiceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationService"/> class.
        /// </summary>
        public OrganizationService() :base()
        {}

        public IEnumerable<Organization> GetOrganizations( string username, string password )
        {
            string errorResponse = string.Empty;
            return GetOrganizations( username, password, out errorResponse );
        }
        
        public IEnumerable<Organization> GetOrganizations( string username, string password, out string errorResponse )
        {
            errorResponse = string.Empty;

            // setup REST call
            var client = new RestClient( _rockStoreUrl );
            client.Timeout = _clientTimeout;
            string requestUrl = string.Format("api/Store/RetrieveOrganizations/{0}/{1}", username, password);
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
