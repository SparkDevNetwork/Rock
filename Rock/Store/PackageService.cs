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


        /*/// <summary>
        /// Gets a list of packages from the store with the ability to provide custom OData filters.
        /// </summary>
        /// <returns>a <see cref="T:IEumerable<Promos>"/> of promos.</returns>
        public List<Package> GetPackages( int? categoryId, List<Parameter> requestParms )
        {
            // setup REST call
            var client = new RestClient( _rockStoreUrl );

            var request = new RestRequest( "Api/Packages", Method.GET );
            request.AddParameter( "$expand", "PrimaryCategory,SecondaryCategory,PackageTypeValue,Vendor,PackageIconBinaryFile", ParameterType.QueryString );

            if ( categoryId.HasValue )
            {
                request.AddParameter( "$filter", String.Format( "(PrimaryCategoryId eq {0}) or (SecondaryCategoryId eq {0})", categoryId.Value.ToString() ) );
            }
            else
            {
                request.AddParameter( "$filter", "(PrimaryCategoryId eq null) or (SecondaryCategoryId eq null)" );
            }

            foreach ( var parm in requestParms )
            {
                requestParms.Add( parm );
            }

            // deserialize to list of packages
            var packages = client.Execute<List<Package>>( request ).Data;

            return packages;
        }*/

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
            // setup REST call
            var client = new RestClient( _rockStoreUrl );
            var request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = string.Format( "Api/Packages/{0}", packageId.ToString() );

            var package = client.Execute<Package>( request ).Data;
            return package;
        }
    }
}
