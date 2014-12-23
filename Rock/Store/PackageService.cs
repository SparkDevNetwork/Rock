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
    /// Service class for the store package model.
    /// </summary>
    public class PackageService : StoreService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageService"/> class.
        /// </summary>
        public PackageService() : base()
        {}

        /// <summary>
        /// Gets a list of packages from the store.
        /// </summary>
        /// <returns>a <see cref="T:IEumerable<Package>"/> of packages.</returns>
        public IEnumerable<Package> GetAllPackages(int? categoryId)
        {
            // setup REST call
            var client = new RestClient( _rockStoreUrl );
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
            var packages = client.Execute<List<Package>>( request ).Data;

            return packages;
        }

        /// <summary>
        /// Gets a package from the store.
        /// </summary>
        /// <returns>a <see cref="Package"/> of the package.</returns>
        public Package GetPackage( int packageId )
        {

            var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
            string storeKey = globalAttributes.GetValue( "RockStoreKey" );
            
            // setup REST call
            var client = new RestClient( _rockStoreUrl );
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = string.Format( "api/Packages/GetPackageDetails/{0}/{1}", packageId.ToString(), storeKey );

            var package = client.Execute<Package>( request ).Data;
            return package;
        }

        /// <summary>
        /// Gets a package from the store.
        /// </summary>
        /// <returns>a <see cref="Package"/> of the package.</returns>
        public List<Package> GetPurchasedPackages( )
        {

            var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
            string storeKey = globalAttributes.GetValue( "RockStoreKey" );

            // setup REST call
            var client = new RestClient( _rockStoreUrl );
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = string.Format( "api/Packages/GetPurchasedPackages/{0}", storeKey );

            var packages = client.Execute<List<Package>>( request ).Data;
            return packages;
        }
    }
}
