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
    /// Service class for the store package model.
    /// </summary>
    public class PackageService : StoreServiceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageService"/> class.
        /// </summary>
        public PackageService() : base()
        { }

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
        public IEnumerable<Package> GetAllPackages( int? categoryId, out string errorResponse )
        {
            errorResponse = string.Empty;
            var encodedOrganizationKey = StoreService.GetEncodedOrganizationKey();

            var resourcePath = string.Empty;
            Dictionary<string, List<string>> queryParameters = null;

            if ( categoryId.HasValue )
            {
                resourcePath = $"Api/Packages/GetSummariesByCategory/{categoryId.Value}/{encodedOrganizationKey}";
            }
            else
            {
                resourcePath = "Api/Promos";
                queryParameters = new Dictionary<string, List<string>>
                {
                    { "$expand", new List<string> { "PrimaryCategory,SecondaryCategory,PackageTypeValue,Vendor,PackageIconBinaryFile" } }
                };
            }

            // deserialize to list of packages
            var response = ExecuteRestGetRequest<List<Package>>( resourcePath, queryParameters );
            var packageList = new List<Package>();

            if ( response.ResponseStatus == ResponseStatus.Completed && response.Data != null )
            {
                packageList = response.Data;

                // If the key is null null out the price so it can't be installed.
                if ( encodedOrganizationKey.IsNullOrWhiteSpace() )
                {
                    foreach ( var package in packageList )
                    {
                        if ( !package.IsFree )
                        {
                            package.Price = null;
                        }
                    }
                }
            }
            else
            {
                errorResponse = response.ErrorMessage;
            }

            return packageList;
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

            var storeKey = StoreService.GetOrganizationKey();
            var response = ExecuteRestGetRequest<Package>( $"api/Packages/GetPackageDetails/{packageId}/{storeKey}/true", null );

            var package = new Package();

            if ( response.ResponseStatus == ResponseStatus.Completed )
            {
                package = response.Data;
                // If the key is null null out the price so it can't be installed.
                if ( storeKey.IsNullOrWhiteSpace() )
                {
                    if ( !package.IsFree )
                    {
                        package.Price = null;
                    }
                }
            }
            else
            {
                errorResponse = response.ErrorMessage;
            }
            return package;
        }

        /// <summary>
        /// Gets a package from the store.
        /// </summary>
        /// <returns>a <see cref="Package"/> of the package.</returns>
        /// 
        public List<Package> GetPurchasedPackages()
        {
            string error = null;
            return GetPurchasedPackages( out error );
        }

        /// <summary>
        /// Gets the purchased packages.
        /// </summary>
        /// <param name="errorResponse">The error response.</param>
        /// <returns></returns>
        public List<Package> GetPurchasedPackages( out string errorResponse )
        {
            errorResponse = string.Empty;

            string storeKey = StoreService.GetOrganizationKey();
            if ( string.IsNullOrEmpty( storeKey ) )
            {
                errorResponse = "The 'Store Key' is not configured yet. Please check the Account and ensure it is configured for your organization.";
                return new List<Package>();
            }

            var response = ExecuteRestGetRequest<List<Package>>( $"api/Packages/GetPurchasedPackages/{storeKey}/true" );

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
    }
}
