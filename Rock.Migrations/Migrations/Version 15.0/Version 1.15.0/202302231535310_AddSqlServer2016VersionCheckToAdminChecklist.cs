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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    using Rock.Configuration;
    using Rock.Enums.Configuration;
    using Rock.Utility.Settings;

    /// <summary>
    ///
    /// </summary>
    public partial class AddSqlServer2016VersionCheckToAdminChecklist : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            var is2016OrHigher = CheckSqlServerVersionGreaterThanSqlServer2016();
            if ( !is2016OrHigher )
            {
                RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.ADMINISTRATOR_CHECKLIST, "Upgrade to SQL Server 2016 or Later", "Please remember that starting with v16 Rock will no longer support SQL Server 2014 (see this link<https://community.rockrms.com/connect/ending-support-for-sql-server-2014> for more details).", "6538909A-B75E-46C3-A141-3CA02DD6DE06" );
            }
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //
        }

        public static bool CheckSqlServerVersionGreaterThanSqlServer2016()
        {
            var database = RockApp.Current.GetDatabaseConfiguration();

            var isOk = database.Platform == DatabasePlatform.AzureSql;

            if ( !isOk )
            {
                try
                {
                    var versionParts = database.VersionNumber.Split( '.' );
                    int.TryParse( versionParts[0], out var majorVersion );

                    if ( majorVersion >= 13 )
                    {
                        isOk = true;
                    }
                }
                catch
                {
                    // This would be pretty bad, but regardless we'll just
                    // return the isOk (not) and let the caller proceed.
                }
            }

            return isOk;
        }
    }
}
