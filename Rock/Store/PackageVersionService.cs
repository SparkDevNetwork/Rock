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
    public class PackageVersionService : StoreService
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
        public PackageVersion GetPackageVersion(int versionId)
        {
            // setup REST call
            var client = new RestClient( _rockStoreUrl );
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = string.Format( "api/Packages/GetPackageVersionDetails/{0}", versionId.ToString() );

            var version = client.Execute<PackageVersion>( request ).Data;
            return version;
        }
    }
}
