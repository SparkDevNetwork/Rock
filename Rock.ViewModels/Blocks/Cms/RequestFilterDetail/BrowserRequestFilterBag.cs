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

namespace Rock.ViewModels.Blocks.Cms.RequestFilterDetail
{
    /// <summary>
    /// Browser filter configuration item.
    /// </summary>
    public class BrowserRequestFilterBag
    {
        /// <summary>
        /// Gets or sets the browser family enum value.
        /// </summary>
        public int BrowserFamily { get; set; }

        /// <summary>
        /// Gets or sets the version comparison type (Reporting.ComparisonType values).
        /// </summary>
        public int VersionComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the browser major version to compare.
        /// </summary>
        public int MajorVersion { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public Guid Guid { get; set; }
    }
}
