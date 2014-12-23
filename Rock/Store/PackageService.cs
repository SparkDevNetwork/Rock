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
        public IEnumerable<Package> GetAllPackages( int? categoryId )
        {
            string error = null;
            return GetAllPackages( categoryId, out error );
        }
        
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


        public Package GetPackage( int packageId, out string errorResponse )
        {
            errorResponse = string.Empty;
            
            var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
            string storeKey = globalAttributes.GetValue( "RockStoreKey" );
            
            // setup REST call
            var client = new RestClient( _rockStoreUrl );
            client.Timeout = _clientTimeout;
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = string.Format( "api/Packages/GetPackageDetails/{0}/{1}", packageId.ToString(), storeKey );

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

        public List<Package> GetPurchasedPackages(out string errorResponse )
        {
            errorResponse = string.Empty;

            var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
            string storeKey = globalAttributes.GetValue( "RockStoreKey" );

            // setup REST call
            var client = new RestClient( _rockStoreUrl );
            client.Timeout = _clientTimeout;
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = string.Format( "api/Packages/GetPurchasedPackages/{0}", storeKey );

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
    }
}
