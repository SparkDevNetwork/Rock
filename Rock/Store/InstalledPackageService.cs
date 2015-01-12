using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestSharp;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;
using System.Web;
using Rock.Web.UI;


namespace Rock.Store
{
    /// <summary>
    /// Service class for the store category model.
    /// </summary>
    public class InstalledPackageService : StoreServiceBase
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Rock.Model.CategoryService" /> class.
        /// </summary>
        public InstalledPackageService() :base()
        {}

        /// <summary>
        /// Gets the installed packages.
        /// </summary>
        /// <returns></returns>
        public static List<InstalledPackage> GetInstalledPackages()
        {
            string packageFile = HttpContext.Current.Server.MapPath( "~/App_Data/InstalledStorePackages.json" );

            try
            {
                using ( StreamReader r = new StreamReader( packageFile ) )
                {
                    string json = r.ReadToEnd();
                    return JsonConvert.DeserializeObject<List<InstalledPackage>>( json );
                }
            }
            catch 
            {
                return new List<InstalledPackage>();
            }
        }

        /// <summary>
        /// Installeds the package version.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <returns></returns>
        public static InstalledPackage InstalledPackageVersion( int packageId )
        {
            var installedPackages = GetInstalledPackages();

            return installedPackages.Where( p => p.PackageId == packageId ).FirstOrDefault();
        }

    }
}
