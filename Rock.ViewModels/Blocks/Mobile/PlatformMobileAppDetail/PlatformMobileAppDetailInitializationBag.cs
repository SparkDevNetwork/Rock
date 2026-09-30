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

using Rock.Enums.Mobile;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Mobile.PlatformMobileAppDetail
{
    /// <summary>
    /// The state the Platform Mobile App Detail block shows: where the platform mobile
    /// application stands against this Rock's builder, and the last run's report.
    /// </summary>
    public class PlatformMobileAppDetailInitializationBag
    {
        /// <summary>
        /// Gets or sets the build state, which decides the one builder action offered.
        /// </summary>
        public PlatformMobileAppBuildState State { get; set; }

        /// <summary>
        /// Gets or sets the version stamped on the platform Site, or null if it is not built.
        /// </summary>
        public string StampVersion { get; set; }

        /// <summary>
        /// Gets or sets the top of the builder ladder compiled into this Rock.
        /// </summary>
        public string CurrentDefinitionVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person may run the builder.
        /// </summary>
        public bool IsEditable { get; set; }

        /// <summary>
        /// Gets or sets the report of the last run, if there has been one.
        /// </summary>
        public PlatformMobileAppRunReportBag LastRun { get; set; }

        /// <summary>
        /// Gets or sets the church owned settings, or null if the app is not built yet.
        /// </summary>
        public PlatformMobileAppSettingsBag Settings { get; set; }

        /// <summary>
        /// Gets or sets the content collections the church can pick from.
        /// </summary>
        public List<ListItemBag> ContentCollectionOptions { get; set; }
    }
}
