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

namespace Rock.ViewModels.Blocks.Administration.SystemInformation
{
    /// <summary>
    /// Contains the diagnostics information displayed on the Diagnostics tab of the
    /// System Information block. Loaded on demand when the tab is first activated.
    /// </summary>
    public class SystemDiagnosticsBag
    {
        /// <summary>
        /// Gets or sets the information about the current Rock database.
        /// </summary>
        public DatabaseInformationBag Database { get; set; }

        /// <summary>
        /// Gets or sets the name of the current Lava engine.
        /// </summary>
        public string LavaEngineName { get; set; }

        /// <summary>
        /// Gets or sets the formatted system (server) date and time.
        /// </summary>
        public string SystemDateTime { get; set; }

        /// <summary>
        /// Gets or sets the formatted current Rock date and time.
        /// </summary>
        public string RockTime { get; set; }

        /// <summary>
        /// Gets or sets the formatted start time of the current process.
        /// </summary>
        public string ProcessStartTime { get; set; }

        /// <summary>
        /// Gets or sets the formatted time the Rock application last started.
        /// </summary>
        public string RockApplicationStartTime { get; set; }

        /// <summary>
        /// Gets or sets the formatted Rock installation date and time.
        /// </summary>
        public string InstallDateTime { get; set; }

        /// <summary>
        /// Gets or sets the name of the machine hosting the application.
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// Gets or sets the file system location of the executing assembly.
        /// </summary>
        public string ExecutingLocation { get; set; }

        /// <summary>
        /// Gets or sets the web root path of the application.
        /// </summary>
        public string WebRootPath { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the last core migration that was run.
        /// </summary>
        public string LastCoreMigration { get; set; }

        /// <summary>
        /// Gets or sets the most recent migration run for each plugin assembly.
        /// </summary>
        public List<PluginMigrationBag> PluginMigrations { get; set; }

        /// <summary>
        /// Gets or sets the counts of the standard queued transactions, grouped by type.
        /// </summary>
        public List<TransactionQueueStatBag> TransactionQueue { get; set; }

        /// <summary>
        /// Gets or sets the registered routes and the pages they serve.
        /// </summary>
        public List<RouteInformationBag> Routes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether cache statistics collection is enabled.
        /// </summary>
        public bool IsCacheStatisticsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the per-cache statistics. Only populated when statistics are enabled.
        /// </summary>
        public List<CacheStatisticBag> CacheStatistics { get; set; }

        /// <summary>
        /// Gets or sets information about the application's worker thread usage.
        /// </summary>
        public ThreadInformationBag Threads { get; set; }
    }
}
