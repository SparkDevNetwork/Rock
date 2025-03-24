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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.LogSettings
{
    /// <summary>
    /// The item details for the Rock Settings block.
    /// </summary>
    public class LogSettingsBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the log level to use for all categories that are
        /// enabled for standard logging.
        /// </summary>
        /// <value>The standard log level.</value>
        public string StandardLogLevel { get; set; }

        /// <summary>
        /// Gets or sets the standard categories to log. These will be logged
        /// at the level specified by <see cref="StandardLogLevel"/>.
        /// </summary>
        /// <value>The standard categories to log.</value>
        public List<string> Categories { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Rock will write logs to the
        /// local file system.
        /// </summary>
        /// <value><c>true</c> if Rock will write logs to the local file system; otherwise, <c>false</c>.</value>
        public bool IsLocalLoggingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Rock will write logs to the
        /// Observability framework.
        /// </summary>
        /// <value><c>true</c> if Rock will write logs to the Observability framework; otherwise, <c>false</c>.</value>
        public bool IsObservabilityLoggingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the advanced settings as a JSON object that conforms
        /// to the standard Microsoft logging syntax. The root object should be
        /// the "Logging" node - meaning "LogLevel" should be one of the keys
        /// defined at the root of this object.
        /// </summary>
        /// <see href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/" />
        /// <value>The advanced settings.</value>
        public string AdvancedSettings { get; set; }


        /// <summary>
        /// Gets or sets the maximum size of the file.
        /// </summary>
        /// <value>
        /// The maximum size of the file.
        /// </value>
        public string MaxFileSize { get; set; }

        /// <summary>
        /// Gets or sets the number of log files.
        /// </summary>
        /// <value>
        /// The number of log files.
        /// </value>
        public string NumberOfLogFiles { get; set; }

        /// <summary>
        /// Gets or sets the custom categories.
        /// </summary>
        /// <value>
        /// The custom categories.
        /// </value>
        public List<string> CustomCategories { get; set; }

        /// <summary>
        /// Gets or sets the selected categories.
        /// </summary>
        /// <value>
        /// The selected categories.
        /// </value>
        public List<string> SelectedCategories { get; set; }
    }
}
