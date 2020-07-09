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
using System.Linq;
using System.Web.Hosting;
using Rock.Store;

namespace Rock.Migrations
{
    /// <summary>
    ///
    /// </summary>
    public partial class PrayerRequestWorkflowAction : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(
                @"DELETE FROM 
                    [PluginMigration]
                WHERE
                    [PluginAssemblyName] = 'org.sparkdevnetwork.PrayerRequestWorkflowAction'" );

            var installedPackages = InstalledPackageService.GetInstalledPackages();
            if ( installedPackages != null && installedPackages.Any( a => a.PackageId == 131 ) )
            {
                installedPackages.RemoveAll( a => a.PackageId == 131 );
                var packageFile = HostingEnvironment.MapPath( "~/App_Data/InstalledStorePackages.json" );
                var packagesAsJson = installedPackages.ToJson();
                System.IO.File.WriteAllText( packageFile, packagesAsJson );
            }
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
