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
    public class PackageCategoryService : StoreServiceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageCategoryService"/> class.
        /// </summary>
        public PackageCategoryService() :base()
        {}

        /// <summary>
        /// Gets the categories.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PackageCategory> GetCategories()
        {
            string error = null;
            return GetCategories( out error );
        }

        /// <summary>
        /// Gets the categories.
        /// </summary>
        /// <param name="errorResponse">The error response.</param>
        /// <returns></returns>
        public IEnumerable<PackageCategory> GetCategories(out string errorResponse)
        {
            errorResponse = string.Empty;

            // setup REST call
            var client = new RestClient(_rockStoreUrl);
            client.Timeout = _clientTimeout;
            string requestUrl = "api/PackageCategories/List";
            var request = new RestRequest(requestUrl, Method.GET);

            // deserialize to list of packages
            var response = client.Execute<List<PackageCategory>>(request);

            if ( response.ResponseStatus == ResponseStatus.Completed )
            {
                return response.Data;
            }
            else
            {
                errorResponse = response.ErrorMessage;
                return new List<PackageCategory>();
            }
        }
    }
}
