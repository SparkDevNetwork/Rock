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
        /// Returns the installed package version.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <returns></returns>
        public static InstalledPackage InstalledPackageVersion( int packageId )
        {
            var installedPackages = GetInstalledPackages();

            return installedPackages.Where( p => p.PackageId == packageId ).FirstOrDefault();
        }


        /// <summary>
        /// Logs that an install occurred.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="versionId">The package version identifier.</param>
        /// <returns></returns>
        public static void SaveInstall( int packageId, string packageName, int versionId, string versionLabel, int vendorId, string vendorName )
        {
            var installedPackages = GetInstalledPackages();

            var package = installedPackages.Where( p => p.PackageId == packageId ).FirstOrDefault();

            // set properties
            package.VersionId = versionId;
            package.VersionLabel = versionLabel;
            package.InstallDateTime = RockDateTime.Now;

            // save results to file
            SaveInstalledPackages( installedPackages );
        }

        private static void SaveInstalledPackages( List<InstalledPackage> packages )
        {
            string packageFile = HttpContext.Current.Server.MapPath( "~/App_Data/InstalledStorePackages.json" );
            
            string packagesAsJson = packages.ToJson();

            System.IO.File.WriteAllText( packageFile, packagesAsJson );

        }
    }
}
