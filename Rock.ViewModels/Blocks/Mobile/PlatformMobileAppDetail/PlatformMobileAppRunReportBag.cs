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

using System;
using System.Collections.Generic;

using Rock.Enums.Mobile;

namespace Rock.ViewModels.Blocks.Mobile.PlatformMobileAppDetail
{
    /// <summary>
    /// What one run of the platform mobile application builder did.
    /// </summary>
    public class PlatformMobileAppRunReportBag
    {
        /// <summary>
        /// Gets or sets which kind of run this was.
        /// </summary>
        public PlatformMobileAppRunMode Mode { get; set; }

        /// <summary>
        /// Gets or sets when the run started.
        /// </summary>
        public DateTimeOffset? StartedDateTime { get; set; }

        /// <summary>
        /// Gets or sets when the run finished.
        /// </summary>
        public DateTimeOffset? CompletedDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether every version completed and the application deployed.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets one entry per ladder version the run attempted.
        /// </summary>
        public List<PlatformMobileAppVersionReportBag> Versions { get; set; }

        /// <summary>
        /// Gets or sets the records on the platform Site that the builder does not recognize.
        /// </summary>
        public List<string> UnexpectedRecords { get; set; }

        /// <summary>
        /// Gets or sets the deploy error, if deploying failed.
        /// </summary>
        public string DeployError { get; set; }
    }
}
