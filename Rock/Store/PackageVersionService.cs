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
    public class PackageVersionService : StoreServiceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryService"/> class.
        /// </summary>
        public PackageVersionService() :base()
        {}

        /// <summary>
        /// Gets a package version.
        /// </summary>
        /// <returns>a <see cref="T:PackageVersion"/> package version.</returns>
        public PackageVersion GetPackageVersion( int versionId )
        {
            string error = null;
            return GetPackageVersion( versionId, out error );
        }

        public PackageVersion GetPackageVersion( int versionId, out string errorResponse )
        {
            errorResponse = string.Empty;
            
            // setup REST call
            var client = new RestClient( _rockStoreUrl );
            client.Timeout = _clientTimeout;
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = string.Format( "api/Packages/GetPackageVersionDetails/{0}", versionId.ToString() );

            var response = client.Execute<PackageVersion>( request );

            if ( response.ResponseStatus == ResponseStatus.Completed )
            {
                return response.Data;
            }
            else
            {
                errorResponse = response.ErrorMessage;
                return new PackageVersion();
            }

        }
    }
}
