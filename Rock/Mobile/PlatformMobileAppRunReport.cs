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
using System.Linq;

using Rock.Enums.Mobile;

namespace Rock.Mobile
{
    /// <summary>
    /// What one run of <see cref="PlatformMobileAppBuilder"/> did. Returned to the control
    /// panel and also stored as JSON on the platform Site under
    /// <see cref="SystemKey.MetadataKey.PlatformMobileAppLastRun"/>, so it survives sessions
    /// and web-farm nodes.
    /// </summary>
    internal class PlatformMobileAppRunReport
    {
        /// <summary>
        /// Gets or sets which kind of run this was.
        /// </summary>
        public PlatformMobileAppRunMode Mode { get; set; }

        /// <summary>
        /// Gets or sets when the run started.
        /// </summary>
        public DateTime StartedDateTime { get; set; }

        /// <summary>
        /// Gets or sets when the run finished.
        /// </summary>
        public DateTime? CompletedDateTime { get; set; }

        /// <summary>
        /// Gets or sets one entry per ladder version the run attempted, in order.
        /// </summary>
        public List<PlatformMobileAppVersionReport> Versions { get; set; } = new List<PlatformMobileAppVersionReport>();

        /// <summary>
        /// Gets or sets the layouts, pages and blocks found on the platform Site that are not
        /// in the builder's Guid registry. They are listed, never deleted: they can only have
        /// arrived outside the supported admin UI.
        /// </summary>
        public List<string> UnexpectedRecords { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the error from deploying the application after the run, if deploying
        /// failed.
        /// </summary>
        public string DeployError { get; set; }

        /// <summary>
        /// Gets a value indicating whether every attempted version completed and the
        /// application deployed.
        /// </summary>
        public bool IsSuccess => Versions.All( v => v.Error == null ) && DeployError == null;
    }

    /// <summary>
    /// What one ladder version did during a run of <see cref="PlatformMobileAppBuilder"/>.
    /// </summary>
    internal class PlatformMobileAppVersionReport
    {
        /// <summary>
        /// Gets or sets the ladder version, such as "21.0".
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets a description of each record this version created.
        /// </summary>
        public List<string> Created { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a description of each existing record this version changed. Empty on
        /// a healthy install that is only being repaired.
        /// </summary>
        public List<string> Updated { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the error that stopped this version, if it failed. A failed version
        /// is rolled back and is not stamped; the run stops there.
        /// </summary>
        public string Error { get; set; }
    }
}
