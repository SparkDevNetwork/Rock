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

namespace Rock.ViewModels.Blocks.Mobile.PlatformMobileAppDetail
{
    /// <summary>
    /// What one ladder version did during a run of the platform mobile application builder.
    /// </summary>
    public class PlatformMobileAppVersionReportBag
    {
        /// <summary>
        /// Gets or sets the ladder version, such as "21.0".
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets a description of each record this version created.
        /// </summary>
        public List<string> Created { get; set; }

        /// <summary>
        /// Gets or sets a description of each existing record this version changed.
        /// </summary>
        public List<string> Updated { get; set; }

        /// <summary>
        /// Gets or sets the error that stopped this version, if it failed.
        /// </summary>
        public string Error { get; set; }
    }
}
