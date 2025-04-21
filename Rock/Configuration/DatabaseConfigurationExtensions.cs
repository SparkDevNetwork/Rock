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

using Rock.Attribute;

namespace Rock.Configuration
{
    /// <summary>
    /// Extension methods for <see cref="IDatabaseConfiguration"/>.
    /// </summary>
    public static class DatabaseConfigurationExtensions
    {
        /// <summary>
        /// Gets a user-friendly name based on the database version number.
        /// </summary>
        /// <param name="database">The database configuration.</param>
        /// <returns>A string representing the user-friendly version name.</returns>
        public static string GetVersionFriendlyName( this IDatabaseConfiguration database )
        {
            if ( database.VersionNumber == null )
            {
                return "Unknown";
            }

            if ( database.VersionNumber.StartsWith( "11.0" ) )
            {
                return "SQL Server 2012";
            }
            else if ( database.VersionNumber.StartsWith( "12.0" ) )
            {
                return "SQL Server 2014";
            }
            else if ( database.VersionNumber.StartsWith( "13.0" ) )
            {
                return "SQL Server 2016";
            }
            else if ( database.VersionNumber.StartsWith( "14.0" ) )
            {
                return "SQL Server 2017";
            }
            else if ( database.VersionNumber.StartsWith( "15.0" ) )
            {
                return "SQL Server 2019";
            }
            else if ( database.VersionNumber.StartsWith( "16.0" ) )
            {
                return "SQL Server 2022";
            }
            else
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Returns the friendly name of the compatibility level for the
        /// specified database configuration. This is returned in the same
        /// format as <see cref="GetVersionFriendlyName(IDatabaseConfiguration)"/>.
        /// </summary>
        /// <param name="database">The database configuration.</param>
        /// <returns>The friendly name of the compatibility level.</returns>
        public static string GetCompatibilityLevelFriendlyName( this IDatabaseConfiguration database )
        {
            switch ( database.CompatibilityLevel )
            {
                case 160:
                    return "SQL Server 2022";
                case 150:
                    return "SQL Server 2019";
                case 140:
                    return "SQL Server 2017";
                case 130:
                    return "SQL Server 2016";
                case 120:
                    return "SQL Server 2014";
                case 110:
                    return "SQL Server 2012";
                case 100:
                    return "SQL Server 2008";
                case 90:
                    return "SQL Server 2005";
                case 80:
                    return "SQL Server 2000";
                default:
                    return database.CompatibilityLevel.ToString();
            }
        }
    }
}
