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

using System;
using Rock.Data;
using Rock.Update.Enum;
using Rock.Update.Exceptions;

namespace Rock.Update.Helpers
{
    /// <summary>
    /// This class is use to validate the a specific rock version can be installed.
    /// </summary>
    public static class VersionValidationHelper
    {
        /// <summary>
        /// Checks the .NET Framework version and returns Pass, Fail, or Unknown which can be
        /// used to determine if it's safe to proceed.
        /// </summary>
        /// <returns>One of the values of the VersionCheckResult enum.</returns>
        public static DotNetVersionCheckResult CheckFrameworkVersion()
        {
            var result = DotNetVersionCheckResult.Fail;
            try
            {
                // Once we get to 4.5 Microsoft recommends we test via the Registry...
                result = RockUpdateHelper.CheckDotNetVersionFromRegistry();
            }
            catch
            {
                // This would be pretty bad, but regardless we'll return
                // the Unknown result and let the caller proceed with caution.
                result = DotNetVersionCheckResult.Unknown;
            }

            return result;
        }

        /// <summary>
        /// Determines whether this instance [can install version] the specified target version.
        /// </summary>
        /// <param name="targetVersion">The target version.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can install version] the specified target version; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanInstallVersion( Version targetVersion )
        {
            try
            {
                ValidateVersionInstall( targetVersion );
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks the SQL server version and returns false if not at the needed
        /// level to proceed.
        /// </summary>
        /// <returns></returns>
        public static bool CheckSqlServerVersionGreaterThenSqlServer2012()
        {
            var isOk = false;
            var sqlVersion = string.Empty;

            try
            {
                sqlVersion = DbService.ExecuteScaler( "SELECT SERVERPROPERTY('productversion')" ).ToString();
                var versionParts = sqlVersion.Split( '.' );

                int.TryParse( versionParts[0], out var majorVersion );

                if ( majorVersion > 11 )
                {
                    isOk = true;
                }
            }
            catch
            {
                // This would be pretty bad, but regardless we'll just
                // return the isOk (not) and let the caller proceed.
            }

            return isOk;
        }

        /// <summary>
        /// Validates the version install.
        /// </summary>
        /// <param name="targetVersion">The target version.</param>
        /// <exception cref="Rock.Update.VersionValidationException">
        /// Version {targetVersion} requires .Net 4.7.2 or greater.
        /// or
        /// Version {targetVersion} requires Microsoft Sql Server 2012 or greater.
        /// </exception>
        public static void ValidateVersionInstall( Version targetVersion )
        {
            var requiresSqlServer14OrHigher = targetVersion.Major > 1 || targetVersion.Minor > 10;
            var requiresNet472 = targetVersion.Major > 1 || targetVersion.Minor > 12;

            if ( !requiresSqlServer14OrHigher )
            {
                return;
            }

            var hasSqlServer2012OrGreater = CheckSqlServerVersionGreaterThenSqlServer2012();
            if ( requiresNet472 )
            {
                var result = CheckFrameworkVersion();
                if ( result == DotNetVersionCheckResult.Fail )
                {
                    throw new VersionValidationException( $"Version {targetVersion} requires .Net 4.7.2 or greater." );
                }
            }

            if ( !hasSqlServer2012OrGreater )
            {
                throw new VersionValidationException( $"Version {targetVersion} requires Microsoft Sql Server 2012 or greater." );
            }
        }

        /// <summary>
        /// Gets the installed version.
        /// </summary>
        /// <returns></returns>
        public static Version GetInstalledVersion()
        {
            return new Version( VersionInfo.VersionInfo.GetRockSemanticVersionNumber() );
        }
    }
}
