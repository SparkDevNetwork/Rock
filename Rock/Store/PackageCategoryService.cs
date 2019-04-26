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
using System.Collections.Generic;

using RestSharp;


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
