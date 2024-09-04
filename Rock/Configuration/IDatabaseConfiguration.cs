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

using Rock.Enums.Configuration;

namespace Rock.Configuration
{
    /// <summary>
    /// The information on the configuration of the database that this Rock
    /// instance is connected to.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>Plugins should not implement this interface as new properties
    ///         will be added over time.</strong>
    ///     </para>
    /// </remarks>
    public interface IDatabaseConfiguration
    {
        /// <summary>
        /// Gets a value indicating whether the database is available and ready
        /// to be accessed. If this value is <c>false</c> then no attempt to
        /// create a <see cref="Data.RockContext"/> should be made.
        /// </summary>
        bool IsDatabaseAvailable { get; }

        /// <summary>
        /// Gets the database platform which is hosting the Rock database.
        /// </summary>
        DatabasePlatform Platform { get; }

        /// <summary>
        /// Gets a flag indicating if READ COMMITTED SNAPSHOT isolation level
        /// is enabled for the database. If this isolation level is enabled,
        /// the database does not hold record locks during the reading phase
        /// of a transaction.
        /// </summary>
        bool IsReadCommittedSnapshotEnabled { get; }

        /// <summary>
        /// Gets a flag indicating if snapshot isolation is enabled for the
        /// database. If this feature is available, each transaction operates
        /// on a snapshot of the database in isolation from other concurrent
        /// operations.
        /// </summary>
        bool IsSnapshotIsolationAllowed { get; }

        /// <summary>
        /// Gets the size of the database, measured in megabytes (MB). This
        /// does not include the size taken up by the log.
        /// </summary>
        decimal? GetDatabaseSize();

        /// <summary>
        /// Gets the size of the database log, measured in megabytes (MB).
        /// </summary>
        decimal? GetLogSize();

        /// <summary>
        /// <para>
        /// Gets the database platform version number string.
        /// </para>
        /// <para>
        /// On SQL Server this would be something like <c>16.0.1115.1</c>.
        /// </para>
        /// </summary>
        string VersionNumber { get; }

        /// <summary>
        /// Gets the database platform version string. This is the raw text
        /// provided by the database to describe the version.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Gets a description of the database server edition or product
        /// variant. The format is specific to each platform.
        /// </summary>
        string Edition { get; }

        /// <summary>
        /// Gets a description of the database server recovery model. The format
        /// is specific to each platform.
        /// </summary>
        string RecoveryModel { get; }

        /// <summary>
        /// Gets a description of the expected capability of the database
        /// platform, or null if the capability cannot be determined. The format
        /// is specific to each platform.
        /// </summary>
        string ServiceObjective { get; }

        /// <summary>
        /// Get the name of the operating system on which the database
        /// server is hosted. The format is specific to each platform.
        /// </summary>
        string DatabaseServerOperatingSystem { get; }

        /// <summary>
        /// Gets the name of the database server connected to.
        /// </summary>
        string ServerName { get; }

        /// <summary>
        /// Gets the name of the database hosting the Rock database.
        /// </summary>
        string DatabaseName { get; }

        /// <summary>
        /// Gets the compatibility level of the database. This is used by
        /// the database platform to indicate that even though it might be
        /// running on version X, it is emulating version Y for compatibility.
        /// </summary>
        int CompatibilityLevel { get; }
    }
}
