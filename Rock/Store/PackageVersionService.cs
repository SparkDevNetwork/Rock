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
using RestSharp;


namespace Rock.Store
{
    /// <summary>
    /// Service class for the store category model.
    /// </summary>
    public class PackageVersionService : StoreServiceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rock.Model.CategoryService"/> class.
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

        /// <summary>
        /// Gets the package version.
        /// </summary>
        /// <param name="versionId">The version identifier.</param>
        /// <param name="errorResponse">The error response.</param>
        /// <returns></returns>
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
