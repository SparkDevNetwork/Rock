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

namespace Rock.ViewModels.Blocks.Administration.SystemInformation
{
    /// <summary>
    /// Information about the current Rock database, displayed on the Diagnostics tab.
    /// </summary>
    public class DatabaseInformationBag
    {
        /// <summary>
        /// Gets or sets the database (catalog) name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the database server name.
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// Gets or sets the full database version string.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the user-friendly database version name. Null when the platform is Azure SQL.
        /// </summary>
        public string FriendlyVersion { get; set; }

        /// <summary>
        /// Gets or sets the user-friendly database compatibility level name.
        /// </summary>
        public string CompatibilityVersion { get; set; }

        /// <summary>
        /// Gets or sets the database size in megabytes.
        /// </summary>
        public decimal? DatabaseSizeMb { get; set; }

        /// <summary>
        /// Gets or sets the transaction log size in megabytes.
        /// </summary>
        public decimal? LogSizeMb { get; set; }

        /// <summary>
        /// Gets or sets the database recovery model.
        /// </summary>
        public string RecoveryModel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether snapshot isolation is allowed.
        /// </summary>
        public bool AllowSnapshotIsolation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether read committed snapshot is enabled.
        /// </summary>
        public bool IsReadCommittedSnapshotOn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the database platform is Azure SQL.
        /// </summary>
        public bool IsAzure { get; set; }

        /// <summary>
        /// Gets or sets the Azure service tier objective. Only populated when the platform is Azure SQL.
        /// </summary>
        public string ServiceObjective { get; set; }

        /// <summary>
        /// Gets or sets the updateability status reported by the read-only context.
        /// Null when no read-only connection string is configured.
        /// </summary>
        public string ReadOnlyContextStatus { get; set; }

        /// <summary>
        /// Gets or sets an error message when database information could not be read.
        /// When set, the other properties are not populated.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
