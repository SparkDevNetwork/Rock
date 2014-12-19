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
    public class InstalledPackageService : StoreService
    {

        InstalledPackage _installedPackages = null;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryService"/> class.
        /// </summary>
        public InstalledPackageService() :base()
        {}

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
            catch ( Exception ex )
            {
                return new List<InstalledPackage>();
            }
        }

        public static InstalledPackage InstalledPackageVersion( int packageId )
        {
            var installedPackages = GetInstalledPackages();

            return installedPackages.Where( p => p.PackageId == packageId ).FirstOrDefault();
        }

    }
}
