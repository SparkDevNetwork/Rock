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
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

using Newtonsoft.Json;


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
            string packageFile = HostingEnvironment.MapPath("~/App_Data/InstalledStorePackages.json");

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

            if ( installedPackages != null )
            {
                return installedPackages.Where( p => p.PackageId == packageId ).FirstOrDefault();
            }

            return null;
        }


        /// <summary>
        /// Logs that an install occurred.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="versionId">The package version identifier.</param>
        /// <param name="versionLabel">The version label.</param>
        /// <param name="vendorId">The vendor identifier.</param>
        /// <param name="vendorName">Name of the vendor.</param>
        /// <param name="installedBy">The installed by.</param>
        public static void SaveInstall( int packageId, string packageName, int versionId, string versionLabel, int vendorId, string vendorName, string installedBy )
        {
            var installedPackages = GetInstalledPackages();

            var package = installedPackages.Where( p => p.PackageId == packageId ).FirstOrDefault();

            if ( package == null )
            {
                package = new InstalledPackage();
                installedPackages.Add( package );
                package.PackageId = packageId;
                package.PackageName = packageName;
                package.VendorId = vendorId;
                package.VendorName = vendorName;
            }

            // set properties
            package.VersionId = versionId;
            package.VersionLabel = versionLabel;
            package.InstallDateTime = RockDateTime.Now;
            package.InstalledBy = installedBy;

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
