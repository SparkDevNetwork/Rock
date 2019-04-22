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
    public class PackageVersionRatingService : StoreServiceBase
    {
        /// <summary>
        /// 
        /// </summary>
        public PackageVersionRatingService() :base()
        {}

        /// <summary>
        /// Gets list of ratings for a specific version.
        /// </summary>
        /// <returns>a <see cref="T:PackageVersion"/> package version.</returns>
        public List<PackageVersionRating> GetPackageVersionRatings( int versionId )
        {
            string error = null;
            return GetPackageVersionRatings( versionId, out error );
        }

        /// <summary>
        /// Gets the ratings for a package version.
        /// </summary>
        /// <param name="versionId">The version identifier.</param>
        /// <param name="errorResponse">The error response.</param>
        /// <returns></returns>
        public List<PackageVersionRating> GetPackageVersionRatings( int versionId, out string errorResponse )
        {
            errorResponse = string.Empty;
            
            // setup REST call
            var client = new RestClient( _rockStoreUrl );
            client.Timeout = _clientTimeout;
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = string.Format( "api/PackageVersionRatings?$filter=PackageVersionId eq {0}&$expand=PersonAlias/Person", versionId.ToString() );

            var response = client.Execute<List<PackageVersionRating>>( request );

            if ( response.ResponseStatus == ResponseStatus.Completed )
            {
                return response.Data;
            }
            else
            {
                errorResponse = response.ErrorMessage;
                return new List<PackageVersionRating>();
            }

        }
    }
}
