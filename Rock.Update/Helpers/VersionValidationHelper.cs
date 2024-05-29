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

using Rock.Attribute;
using Rock.Configuration;
using Rock.Data;
using Rock.Update.Enum;
using Rock.Update.Exceptions;
using Rock.Web.Cache;

namespace Rock.Update.Helpers
{
    /// <summary>
    /// This class is use to validate the a specific rock version can be installed.
    /// </summary>
    public static class VersionValidationHelper
    {
        [RockObsolete( "1.16" )]
        [Obsolete( "This class has been replaced with SqlServerCompatibilityLevel." )]
        public static class SqlServerVersion
        {
            public const int v2022 = 16;
            public const int v2019 = 15;
            public const int v2017 = 14;
            public const int v2016 = 13;
            public const int v2014 = 12;
            public const int v2012 = 11;
        }

        /// <summary>
        /// Identifies the Compatibility Level of a database by its version.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.16" )]
        public static class SqlServerCompatibilityLevel
        {
            public const int v2022 = 160;
            public const int v2019 = 150;
            public const int v2017 = 140;
            public const int v2016 = 130;
            public const int v2014 = 120;
            public const int v2012 = 110;
        }

        private const int dotNet472ReleaseNumber = 461808;

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
                // Check if Release is >= 461808 (4.7.2)
                if ( HostingSettings.GetDotNetReleaseNumber() >= dotNet472ReleaseNumber )
                {
                    return DotNetVersionCheckResult.Pass;
                }
                else
                {
                    return DotNetVersionCheckResult.Fail;
                }
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
        [Obsolete( "No longer required after successful update to v1.11.0" )]
        [RockObsolete( "1.11.1" )]
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

            if ( requiresNet472 )
            {
                var result = CheckFrameworkVersion();
                if ( result == DotNetVersionCheckResult.Fail )
                {
                    throw new VersionValidationException( $"Version {targetVersion} requires .Net 4.7.2 or greater." );
                }
            }

            var isTargetVersionGreaterThan15 = targetVersion.Major > 1 || targetVersion.Minor > 15;

            var hasSqlServer2016OrGreater = CheckSqlServerCompatibilityLevel( SqlServerCompatibilityLevel.v2016 );
            if ( !hasSqlServer2016OrGreater && isTargetVersionGreaterThan15 )
            {
                throw new VersionValidationException( $"Version {targetVersion} requires Microsoft SQL Azure or Microsoft Sql Server 2016 or greater." );
            }

            // Read the LavaSupportLevel setting for the current database.
            // The setting is removed by the v1.16 migration process, so this check is only relevant for databases prior to that version.
#pragma warning disable CS0618 // Type or member is obsolete
            var lavaSupportLevel = GlobalAttributesCache.Value( "core.LavaSupportLevel" ).ConvertToEnumOrNull<Lava.LavaSupportLevel>() ?? Lava.LavaSupportLevel.NoLegacy;
            if ( isTargetVersionGreaterThan15 && lavaSupportLevel != Lava.LavaSupportLevel.NoLegacy )
            {
                throw new VersionValidationException( $"Version {targetVersion} requires a Lava Support Level of 'NoLegacy'." );
            }
#pragma warning restore CS0618 // Type or member is obsolete

            var isTargetVersionGreaterThan16 = targetVersion.Major > 1 || targetVersion.Minor > 16;
            if ( isTargetVersionGreaterThan16 && RockApp.Current.GetCurrentLavaEngineName() != "Fluid" )
            {
                throw new VersionValidationException( $"Version {targetVersion} requires the 'Fluid' Lava Engine Liquid Framework." );
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

        /// <summary>
        /// Checks the SQL server compatibility level and returns false if not at the needed
        /// level to proceed.
        /// </summary>
        /// <param name="compatibiltyLevel">The compatibility level required to pass the check.</param>
        /// <returns></returns>
        public static bool CheckSqlServerCompatibilityLevel( int compatibiltyLevel )
        {
            var database = RockApp.Current.GetDatabaseConfiguration();

            var isOk = database.CompatibilityLevel >= compatibiltyLevel;
            return isOk;
        }
    }
}
