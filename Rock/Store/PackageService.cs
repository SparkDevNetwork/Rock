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

using Newtonsoft.Json;

using RestSharp;


namespace Rock.Store
{
    /// <summary>
    /// Service class for the store package model.
    /// </summary>
    public class PackageService : StoreServiceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageService"/> class.
        /// </summary>
        public PackageService() : base()
        {}

        /// <summary>
        /// Gets all packages.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <returns></returns>
        public IEnumerable<Package> GetAllPackages( int? categoryId )
        {
            string error = null;
            return GetAllPackages( categoryId, out error );
        }

        /// <summary>
        /// Gets all packages.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="errorResponse">The error response.</param>
        /// <returns></returns>
        public IEnumerable<Package> GetAllPackages(int? categoryId, out string errorResponse)
        {
            errorResponse = string.Empty;
            
            // setup REST call
            var client = new RestClient( _rockStoreUrl );
            client.Timeout = _clientTimeout;
            var request = new RestRequest();
            request.Method = Method.GET;

            if ( categoryId.HasValue )
            {
                request.Resource = string.Format( "Api/Packages/GetSummariesByCategory/{0}", categoryId.Value.ToString() );
            }
            else
            {
                request.Resource = "Api/Promos";
                request.AddParameter( "$expand", "PrimaryCategory,SecondaryCategory,PackageTypeValue,Vendor,PackageIconBinaryFile", ParameterType.QueryString );
            }

            // deserialize to list of packages
            var response = client.Execute<List<Package>>( request );

            if ( response.ResponseStatus == ResponseStatus.Completed )
            {
                return response.Data;
            }
            else
            {
                errorResponse = response.ErrorMessage;
                return new List<Package>();
            }
            
        }

        /// <summary>
        /// Gets a package from the store.
        /// </summary>
        /// <returns>a <see cref="Package"/> of the package.</returns>

        public Package GetPackage( int packageId )
        {
            string error = null;
            return GetPackage( packageId, out error );
        }


        /// <summary>
        /// Gets the package.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="errorResponse">The error response.</param>
        /// <returns></returns>
        public Package GetPackage( int packageId, out string errorResponse )
        {
            errorResponse = string.Empty;
            
            string storeKey = StoreService.GetOrganizationKey();
            
            // setup REST call
            var client = new RestClient( _rockStoreUrl );
            client.Timeout = _clientTimeout;
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = string.Format( "api/Packages/GetPackageDetails/{0}/{1}", packageId.ToString(), storeKey );

            //request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

            var response = client.Execute<Package>( request );

            if ( response.ResponseStatus == ResponseStatus.Completed )
            {
                return response.Data;
            }
            else
            {
                errorResponse = response.ErrorMessage;
                return new Package();
            }
        }

        /// <summary>
        /// Gets a package from the store.
        /// </summary>
        /// <returns>a <see cref="Package"/> of the package.</returns>
        /// 
        public List<Package> GetPurchasedPackages(  )
        {
            string error = null;
            return GetPurchasedPackages( out error );
        }

        /// <summary>
        /// Gets the purchased packages.
        /// </summary>
        /// <param name="errorResponse">The error response.</param>
        /// <returns></returns>
        public List<Package> GetPurchasedPackages(out string errorResponse )
        {
            errorResponse = string.Empty;

            string storeKey = StoreService.GetOrganizationKey(); ;

            // setup REST call
            var client = new RestClient( _rockStoreUrl );
            client.Timeout = _clientTimeout;
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = string.Format( "api/Packages/GetPurchasedPackages/{0}", storeKey );

            var response = client.Execute( request );

            if ( response.ResponseStatus == ResponseStatus.Completed )
            {
                List<Package> packages = JsonConvert.DeserializeObject<List<Package>>(response.Content);
                return packages;
            }
            else
            {
                errorResponse = response.ErrorMessage;
                return new List<Package>();
            }
        }
    }
}
